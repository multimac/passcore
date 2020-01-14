export interface IChangePasswordFormInitialModel {
    CurrentPassword: string;
    NewPassword: string;
    NewPasswordVerify: string;
    Recaptcha: string;
    Username: string;
    MfaSelection: string;
    MfaPasscode: string;
}

export interface IChangePasswordFormProps {
    submitData: boolean;
    toSubmitData: any;
    parentRef: any;
    onValidated: any;
    shouldReset: boolean;
    shouldResetRecaptcha: boolean;
    changeResetState: any;
    setReCaptchaToken: any;
    ReCaptchaToken: string;
    setMfaOptions: any;
    mfaOptions: any;
}

export interface IPasswordGenProps {
    value: string;
    setValue: any;
}
