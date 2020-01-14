namespace Unosquare.PassCore.Web
{
    using System.Threading.Tasks;
    using Common;

    internal class DisabledMultiFactorAuthProvider : IMultiFactorAuthProvider
    {
        public Task<(ApiMultiFactorAuthOptions?, ApiErrorItem?)> GetOptions(string username)
            => Task.FromResult<(ApiMultiFactorAuthOptions?, ApiErrorItem?)>((null, null));

        public Task<ApiErrorItem?> PerformMultiFactorAuthentication(string username, string type, string? passcode)
            => Task.FromResult<ApiErrorItem?>(new ApiErrorItem(ApiErrorCode.Generic, "Multi-factor authentication is disabled."));
    }
}
