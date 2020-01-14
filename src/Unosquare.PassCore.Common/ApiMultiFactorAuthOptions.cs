namespace Unosquare.PassCore.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the fields multi-factor device options available.
    /// </summary>
    public class ApiMultiFactorAuthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiMultiFactorAuthOptions" /> class.
        /// </summary>
        /// <param name="supportsPasscode">True if the configured multi-factor authentication supports one-time passcodes, otherwise false.</param>
        /// <param name="factors">The factors available for multi-factor authentication.</param>
        public ApiMultiFactorAuthOptions(bool supportsPasscode, IEnumerable<KeyValuePair<string, string>> factors)
        {
            SupportsPasscode = supportsPasscode;
            Factors = factors;
        }

        /// <summary>
        /// Gets or sets whether one-time passcodes are supported.
        /// </summary>
        /// <value>
        /// Whether one-time passcodes are supported.
        /// </value>
        public bool SupportsPasscode { get; }

        /// <summary>
        /// Gets the available factors.
        /// </summary>
        /// <value>
        /// The available factors.
        /// </value>
        public IEnumerable<KeyValuePair<string, string>> Factors { get; }
    }
}
