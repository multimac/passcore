namespace Unosquare.PassCore.Common
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a interface for a multi-factor provider.
    /// </summary>
    public interface IMultiFactorAuthProvider
    {
        /// <summary>
        /// Retrieves the multi-factor authentication options for the given user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The multi-factor authentication options if any, and any errors.</returns>
        Task<(ApiMultiFactorAuthOptions?, ApiErrorItem?)> GetOptions(string username);

        /// <summary>
        /// Performs the multi-factor authentication using the given options.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="type">The type of multi-factor authentication to perform.</param>
        /// <param name="passcode">If provided by the user, the passcode to perform multi-factor authentication.</param>
        /// <returns>The API error item or null if the multi-factor authentication operation was successful.</returns>
        Task<ApiErrorItem?> PerformMultiFactorAuthentication(string username, string type, string? passcode);
    }
}