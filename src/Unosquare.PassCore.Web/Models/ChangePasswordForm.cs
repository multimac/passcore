namespace Unosquare.PassCore.Web.Models
{
    public class ChangePasswordForm
    {
        public string ChangePasswordButtonLabel { get; set; }
        public string CurrentPasswordHelpblock { get; set; }
        public string CurrentPasswordLabel { get; set; }
        public string HelpText { get; set; }
        public string NewPasswordHelpblock { get; set; }
        public string NewPasswordLabel { get; set; }
        public string NewPasswordVerifyHelpblock { get; set; }
        public string NewPasswordVerifyLabel { get; set; }
        public string UsernameDefaultDomainHelperBlock { get; set; }
        public string UsernameHelpblock { get; set; }
        public string UsernameLabel { get; set; }
        public string MfaSelectionHelpblock { get; set; }
        public string MfaSelectionLabel { get; set; }
        public string MfaPasscodeHelpblock {get;set;}
        public string MfaPasscodeLabel {get;set;}
    }
}