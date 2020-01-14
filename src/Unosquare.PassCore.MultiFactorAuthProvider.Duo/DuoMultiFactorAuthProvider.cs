namespace Unosquare.PassCore.MultiFactorAuthProvider.Duo
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using global::Duo;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    /// <inheritdoc />
    /// <summary>
    /// Default Change Password Provider using 'System.DirectoryServices' from Microsoft.
    /// </summary>
    /// <seealso cref="IMultiFactorAuthProvider" />
    public partial class DuoMultiFactorAuthProvider : IMultiFactorAuthProvider
    {
        private const string PushSuffix = ":push";
        private const string SmsSuffix = ":sms";
        private const string PhoneSuffix = ":phone";

        private readonly DuoApi _client;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuoMultiFactorAuthProvider"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        public DuoMultiFactorAuthProvider(
            ILogger<DuoMultiFactorAuthProvider> logger,
            IOptions<DuoMultiFactorAuthOptions> options)
        {
            _logger = logger;

            _client = new DuoApi(
                options.Value.DuoIntegrationKey,
                options.Value.DuoSecretKey,
                options.Value.DuoApiHostname);
        }

        /// <inheritdoc />
        public async Task<(ApiMultiFactorAuthOptions?, ApiErrorItem?)> GetOptions(string username)
        {
            var parameters = new Dictionary<string, string> { { "username", username } };
            var response = _client.JSONApiCall("POST", "/auth/v2/preauth", parameters);

            switch ((string)response["result"])
            {
                case "auth":
                    break;
                case "allow":
                    _logger.LogInformation($"Multi-factor authentication bypassed for {username}");
                    return (null, null);
                case "deny":
                    _logger.LogInformation($"Multi-factor authentication is not permitted for {username}");
                    return (null, new ApiErrorItem(ApiErrorCode.MultiFactorAuthDenied, "User is not permitted to authenticate at this time."));
                case "enroll":
                    _logger.LogInformation($"User {username} requires enrollment before multi-factor authentication can be performed.");
                    return (null, new ApiErrorItem(ApiErrorCode.MultiFactorUnavailable, "User enrollment is required."));

                default:
                    _logger.LogError("Unknown response received from Duo API.");
                    return (null, new ApiErrorItem(ApiErrorCode.Generic, "Unknown response from Duo API."));
            }

            var factors = new Dictionary<string, string>();
            foreach (var info in response["devices"].ToArray())
            {
                var deviceId = (string)info["device"];
                var displayName = (string)info["display_name"];

                var capabilities = info["capabilities"].Select(cap => (string)cap).ToList();
                if (capabilities.Contains("push")) { factors.Add($"{displayName} - Push", deviceId + PushSuffix); }
                if (capabilities.Contains("sms")) { factors.Add($"{displayName} - SMS", deviceId + SmsSuffix); }
                if (capabilities.Contains("phone")) { factors.Add($"{displayName} - Phone Call", deviceId + PhoneSuffix); }
            }

            return (new ApiMultiFactorAuthOptions(true, factors), null);
        }

        /// <inheritdoc />
        public async Task<ApiErrorItem?> PerformMultiFactorAuthentication(string username, string type, string passcode)
        {
            var parameters = new Dictionary<string, string> { { "username", username } };
            if (type == "passcode")
            {
                parameters.Add("factor", "passcode");
                parameters.Add("passcode", passcode);
            }
            else
            {
                string? device = null;
                if (type.EndsWith(PushSuffix))
                {
                    device = type.Substring(0, type.Length - PushSuffix.Length);
                    type = "push";
                }
                else if (type.EndsWith(SmsSuffix))
                {
                    device = type.Substring(0, type.Length - SmsSuffix.Length);
                    type = "sms";
                }
                else if (type.EndsWith(PhoneSuffix))
                {
                    device = type.Substring(0, type.Length - PhoneSuffix.Length);
                    type = "phone";
                }

                if (device == null)
                {
                    return new ApiErrorItem(ApiErrorCode.Generic, "Unknown MFA type.");
                }

                parameters.Add("device", device);
                parameters.Add("factor", type);
            }

            var response = _client.JSONApiCall<Dictionary<string, object>>("POST", "/auth/v2/auth", parameters);
            var result = response["result"] as string;

            switch (result)
            {
                case "allow":
                    return null;
                case "deny":
                    return new ApiErrorItem(ApiErrorCode.MultiFactorAuthDenied, "Multi-factor authentication was denied.");

                default:
                    return new ApiErrorItem(ApiErrorCode.Generic, "Unknown response from Duo API.");
            }
        }
    }
}