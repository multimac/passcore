import Button from '@material-ui/core/Button/Button';
import Paper from '@material-ui/core/Paper/Paper';
import * as React from 'react';
import { LoadingIcon } from 'uno-material-ui';
import { ValidatorForm } from 'uno-react';
import { ChangePasswordDialog } from '../Dialogs/ChangePasswordDialog';
import { GlobalContext, SnackbarContext } from '../Provider/GlobalContext';
import { fetchRequest } from '../Utils/FetchRequest';
import { ChangePasswordForm } from './ChangePasswordForm';

export const ChangePassword: React.FunctionComponent<{}> = () => {
    const [validated, setValidated] = React.useState(false);
    const [submit, setSubmit] = React.useState(false);
    const [requestInProgress, setRequestInProgress] = React.useState(false);
    const [dialogIsOpen, setDialog] = React.useState(false);
    const [token, setToken] = React.useState('');
    const validatorFormRef = React.useRef(null);
    const { alerts, changePasswordForm, recaptcha, validationRegex } = React.useContext(GlobalContext);
    const { changePasswordButtonLabel } = changePasswordForm;
    const { sendMessage } = React.useContext(SnackbarContext);
    const [shouldReset, setReset] = React.useState(false);
    const [shouldResetRecaptcha, setResetRecaptcha] = React.useState(false);
    const [mfaOptions, setMfaOptions] = React.useState(null);

    const onSubmitValidatorForm = () => setSubmit(true);

    const toSubmitData = (formData: {}) => {
        setSubmit(false);
        setRequestInProgress(true);

        fetchRequest('api/password', 'POST', JSON.stringify({ ...formData, Recaptcha: token })).then(
            (response: any) => {
                setResetRecaptcha(!shouldResetRecaptcha);
                setRequestInProgress(false);

                if (response.multiFactorOptions) {
                    setMfaOptions(response.multiFactorOptions);
                    sendMessage(alerts.infoMultiFactorAuthRequired, "info");
                    return;
                } else if (response.errors && response.errors.length) {
                    let errorAlertMessage = '';
                    response.errors.forEach((error: any) => {
                        switch (error.errorCode) {
                            case 0:
                                errorAlertMessage += error.message;
                                break;
                            case 1:
                                errorAlertMessage += alerts.errorFieldRequired;
                                break;
                            case 2:
                                errorAlertMessage += alerts.errorFieldMismatch;
                                break;
                            case 3:
                                errorAlertMessage += alerts.errorInvalidUser;
                                break;
                            case 4:
                                errorAlertMessage += alerts.errorInvalidCredentials;
                                break;
                            case 5:
                                errorAlertMessage += alerts.errorCaptcha;
                                break;
                            case 6:
                                errorAlertMessage += alerts.errorPasswordChangeNotAllowed;
                                break;
                            case 7:
                                errorAlertMessage += alerts.errorInvalidDomain;
                                break;
                            case 8:
                                errorAlertMessage += alerts.errorConnectionLdap;
                                break;
                            case 9:
                                errorAlertMessage += alerts.errorComplexPassword;
                                break;
                            case 10:
                                errorAlertMessage += alerts.errorScorePassowrd;
                                break;
                            case 11:
                                errorAlertMessage += alerts.errorMfaUnavailable;
                                break;
                            case 12:
                                errorAlertMessage += alerts.errorMfaDenied;
                                break;
                        }
                    });

                    sendMessage(errorAlertMessage, 'error');
                    return;
                }
                setDialog(true);
            },
        );
    };

    const onCloseDialog = () => {
        setDialog(false);
        setMfaOptions(null);

        setReset(true);
    };

    ValidatorForm.addValidationRule('isUserName', (value: string) =>
        new RegExp(validationRegex.usernameRegex).test(value),
    );

    ValidatorForm.addValidationRule('isUserEmail', (value: string) =>
        new RegExp(validationRegex.emailRegex).test(value),
    );

    ValidatorForm.addValidationRule('isPasswordMatch', (value: string, comparedValue: any) => value === comparedValue);

    return (
        <>
            <Paper
                style={{
                    borderRadius: '10px',
                    marginTop: '75px',
                    width: '650px',
                    zIndex: 1,
                }}
                elevation={6}
            >
                <ValidatorForm
                    ref={validatorFormRef}
                    autoComplete="off"
                    instantValidate={true}
                    onSubmit={onSubmitValidatorForm}
                >
                    <ChangePasswordForm
                        submitData={submit}
                        toSubmitData={toSubmitData}
                        parentRef={validatorFormRef}
                        onValidated={setValidated}
                        shouldReset={shouldReset}
                        shouldResetRecaptcha={shouldResetRecaptcha}
                        changeResetState={setReset}
                        setReCaptchaToken={setToken}
                        setMfaOptions={setMfaOptions}
                        mfaOptions={mfaOptions}
                        ReCaptchaToken={token}
                    />
                    <Button
                        type="submit"
                        variant="contained"
                        color="primary"
                        disabled={!validated || submit || requestInProgress}
                        style={{
                            marginBottom: "50px",
                            marginLeft: "205px",
                            marginTop: "25px",
                            width: '240px',
                        }}
                    >
                        {changePasswordButtonLabel}
                    </Button>
                    {(submit || requestInProgress) && (
                        <div style={{display: "inline-flex", marginBottom: "50px", width: "100%"}}>
                            <LoadingIcon />
                        </div>)}
                </ValidatorForm>
            </Paper>
            <ChangePasswordDialog open={dialogIsOpen} onClose={onCloseDialog} />
        </>
    );
};
