namespace Unosquare.PassCore.MultiFactorAuthProvider.Duo
{
    using Common;

    /// <summary>
    /// Represents the options of this provider.
    /// </summary>
    /// <seealso cref="Unosquare.PassCore.Common.IAppSettings" />
    public class DuoMultiFactorAuthOptions
    {
        /// <summary>
        /// Gets or sets the API hostname.
        /// </summary>
        /// <value>
        /// The API hostname.
        /// </value>
        public string DuoApiHostname { get; set; }

        /// <summary>
        /// Gets or sets the Duo integration key.
        /// </summary>
        /// <value>
        /// The Duo integration key.
        /// </value>
        public string DuoIntegrationKey { get; set; }

        /// <summary>
        /// Gets or sets the Duo secret key.
        /// </summary>
        /// <value>
        /// The Duo secret key.
        /// </value>
        public string DuoSecretKey { get; set; }
    }
}
