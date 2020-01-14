namespace Unosquare.PassCore.Web.Controllers
{
    using Common;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Models;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Zxcvbn;

    /// <summary>
    /// Represents a controller class holding all of the server-side functionality of this tool.
    /// </summary>
    [Route("api/[controller]")]
    public class PasswordController : Controller
    {
        private readonly ILogger _logger;
        private readonly ClientSettings _options;
        private readonly IPasswordChangeProvider _passwordChangeProvider;
        private readonly IMultiFactorAuthProvider _multiFactorAuthProvider;
        private readonly RNGCryptoServiceProvider? _rngCsp;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordController" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="optionsAccessor">The options accessor.</param>
        /// <param name="passwordChangeProvider">The password change provider.</param>
        public PasswordController(
            ILogger<PasswordController> logger,
            IOptions<ClientSettings> optionsAccessor,
            IPasswordChangeProvider passwordChangeProvider,
            IMultiFactorAuthProvider multiFactorAuthProvider)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
            _passwordChangeProvider = passwordChangeProvider;
            _multiFactorAuthProvider = multiFactorAuthProvider;

            if (_options.UsePasswordGeneration) _rngCsp = new RNGCryptoServiceProvider();
        }

        /// <summary>
        /// Returns the ClientSettings object as a JSON string.
        /// </summary>
        /// <returns>A Json representation of the ClientSettings object.</returns>
        [HttpGet]
        public IActionResult Get() => Json(_options);

        /// <summary>
        /// Returns generated password as a JSON string.
        /// </summary>
        /// <returns>A Json with a password property which contains a random generated password.</returns>
        [HttpGet]
        [Route("generated")]
        public IActionResult GetGeneratedPassword()
        {
            if (_rngCsp == null)
                return NotFound();

            return Json(new { password = PasswordGenerator.Generate(_rngCsp, _options.PasswordEntropy) });
        }

        /// <summary>
        /// Returns the multi-factor options for the given username.
        /// </summary>
        /// <returns>An <see cref="ApiResult" /> containing the multi-factor options.</returns>
        [HttpGet]
        [Route("multi-factor")]
        public async Task<IActionResult> GetMultiFactorOptions([FromQuery] string username)
        {
            var (options, error) = await _multiFactorAuthProvider.GetOptions(username);

            if (error != null)
            {
                var result = new ApiResult();
                result.Errors.Add(error);

                return Json(result);
            }

            return (options != null)
                ? Json(ApiResult.RequiresMultiFactor(options))
                : Json(new ApiResult());
        }

        /// <summary>
        /// Given a POST request, processes and changes a User's password.
        /// </summary>
        /// <param name="model">The value.</param>
        /// <returns>A task representing the async operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChangePasswordModel model)
        {
            // Validate the request
            if (model == null)
            {
                _logger.LogWarning("Null model");

                return BadRequest(ApiResult.InvalidRequest());
            }

            if (model.NewPassword != model.NewPasswordVerify)
            {
                _logger.LogWarning("Invalid model, passwords don't match");

                return BadRequest(ApiResult.InvalidRequest());
            }

            // Validate the model
            if (ModelState.IsValid == false)
            {
                _logger.LogWarning("Invalid model, validation failed");

                return BadRequest(ApiResult.FromModelStateErrors(ModelState));
            }

            // Validate the Captcha
            try
            {
                if (await ValidateRecaptcha(model.Recaptcha).ConfigureAwait(false) == false)
                    throw new InvalidOperationException("Invalid Recaptcha response");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid Recaptcha");
                return BadRequest(ApiResult.InvalidCaptcha());
            }

            var result = new ApiResult();

            try
            {
                if (_options.MinimumScore > 0 && Zxcvbn.MatchPassword(model.NewPassword).Score < _options.MinimumScore)
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.MinimumScore));
                    return BadRequest(result);
                }

                if (!string.IsNullOrEmpty(model.MfaSelection))
                {
                    var multiFactorAuth = await _multiFactorAuthProvider.PerformMultiFactorAuthentication(
                        model.Username,
                        model.MfaSelection,
                        model.MfaPasscode);

                    if (multiFactorAuth != null)
                        result.Errors.Add(multiFactorAuth);
                    else
                    {
                        var resultPasswordChange = _passwordChangeProvider.PerformPasswordChange(
                            model.Username,
                            model.CurrentPassword,
                            model.NewPassword);

                        if (resultPasswordChange == null)
                            return Json(result);

                        result.Errors.Add(resultPasswordChange);
                    }
                }
                else
                {
                    var (options, error) = await _multiFactorAuthProvider.GetOptions(
                        model.Username);

                    if (options != null)
                    {
                        return Json(ApiResult.RequiresMultiFactor(options));
                    }
                    else if (error != null)
                    {
                        result.Errors.Add(error);
                    }
                    else
                    {
                        var resultPasswordChange = _passwordChangeProvider.PerformPasswordChange(
                            model.Username,
                            model.CurrentPassword,
                            model.NewPassword);

                        if (resultPasswordChange == null)
                            return Json(result);

                        result.Errors.Add(resultPasswordChange);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update password");

                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, ex.Message));
            }

            return BadRequest(result);
        }

        private async Task<bool> ValidateRecaptcha(string recaptchaResponse)
        {
            // skip validation if we don't enable recaptcha
            if (string.IsNullOrWhiteSpace(_options.Recaptcha.PrivateKey))
                return true;

            // immediately return false because we don't 
            if (string.IsNullOrEmpty(recaptchaResponse))
                return false;

            var requestUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={_options.Recaptcha.PrivateKey}&response={recaptchaResponse}";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(requestUrl);
            var validationResponse = JObject.Parse(response);

            return validationResponse.ContainsKey("success")
                && validationResponse["success"].ToObject<bool>();
        }
    }
}
