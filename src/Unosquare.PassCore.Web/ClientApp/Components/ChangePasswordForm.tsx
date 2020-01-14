import FormGroup from '@material-ui/core/FormGroup/FormGroup';
import * as React from 'react';
import { TextValidator } from 'uno-material-ui';
import MenuItem from '@material-ui/core/MenuItem';
import Select from '@material-ui/core/Select';
import FormControl from '@material-ui/core/FormControl';
import InputLabel from '@material-ui/core/InputLabel';
import FormHelperText from '@material-ui/core/FormHelperText';
import { useStateForModel } from 'uno-react';
import { GlobalContext } from '../Provider/GlobalContext';
import { IChangePasswordFormInitialModel, IChangePasswordFormProps } from '../types/Components';
import { PasswordGenerator } from './PasswordGenerator';
import { PasswordStrengthBar } from './PasswordStrengthBar';
import { ReCaptcha } from './ReCaptcha';
import { fetchRequest } from '../Utils/FetchRequest';

const defaultState: IChangePasswordFormInitialModel = {
    CurrentPassword: '',
    NewPassword: '',
    NewPasswordVerify: '',
    Recaptcha: '',
    Username: new URLSearchParams(window.location.search).get('userName') || '',
    MfaSelection: '',
    MfaPasscode: '',
};

export const ChangePasswordForm: React.FunctionComponent<IChangePasswordFormProps> = ({
    submitData,
    toSubmitData,
    parentRef,
    onValidated,
    shouldReset,
    changeResetState,
    setReCaptchaToken,
    shouldResetRecaptcha,
    ReCaptchaToken,
    setMfaOptions,
    mfaOptions,
}: IChangePasswordFormProps) => {
    const abortControllerRef = React.useRef(null);
    const [fields, handleChange] = useStateForModel({ ...defaultState });

    const {
        changePasswordForm,
        errorsPasswordForm,
        usePasswordGeneration,
        useEmail,
        showPasswordMeter,
        recaptcha,
    } = React.useContext(GlobalContext);

    const {
        currentPasswordHelpblock,
        currentPasswordLabel,
        newPasswordHelpblock,
        newPasswordLabel,
        newPasswordVerifyHelpblock,
        newPasswordVerifyLabel,
        usernameDefaultDomainHelperBlock,
        usernameHelpblock,
        usernameLabel,
        mfaSelectionHelpblock,
        mfaSelectionLabel,
        mfaPasscodeHelpblock,
        mfaPasscodeLabel,
    } = changePasswordForm;

    const { fieldRequired, passwordMatch, usernameEmailPattern, usernamePattern } = errorsPasswordForm;

    const userNameValidations = ['required', useEmail ? 'isUserEmail' : 'isUserName'];
    const userNameErrorMessages = [fieldRequired, useEmail ? usernameEmailPattern : usernamePattern];
    const userNameHelperText = useEmail ? usernameHelpblock : usernameDefaultDomainHelperBlock;

    if (submitData) {
        if (abortControllerRef.current) {
            abortControllerRef.current.abort()
        }

        toSubmitData(fields);
    }

    React.useEffect(() => {
        if (parentRef.current !== null && parentRef.current.isFormValid !== null) {
            parentRef.current.isFormValid().then((response: any) => {
                let validated = response;
                if (recaptcha.siteKey && recaptcha.siteKey !== '') {
                    validated = validated && ReCaptchaToken !== '';
                }
                onValidated(validated);
            });
        }
    });

    React.useEffect(() => {
        if (shouldReset) {
            handleChange({ ...defaultState });
            changeResetState(false);
            if (parentRef.current && parentRef.current.resetValidations) {
                parentRef.current.resetValidations();
            }
        }
    }, [shouldReset]);

    const setGenerated = (password: any) =>
        handleChange({
            NewPassword: password,
            NewPasswordVerify: password,
        });

    const checkMultiFactor = (e: any) => {
        if (abortControllerRef.current) {
            abortControllerRef.current.abort()
        }

        if (!fields.Username) {
            return;
        }

        abortControllerRef.current = new AbortController();
        fetchRequest(
            `api/password/multi-factor?username=${fields.Username}`,
            'GET',
            null,
            abortControllerRef.current.signal
        ).then(
            (response: any) => {
                abortControllerRef.current = null;
                if (!response || !response.multiFactorOptions) {
                    setMfaOptions(null);
                    return;
                }

                setMfaOptions(response.multiFactorOptions);
            }
        );
    }

    return (
        <FormGroup row={false} style={{ width: '80%', margin: '15px 0 0 10%' }}>
            <TextValidator
                autoFocus={true}
                inputProps={{
                    tabIndex: 1,
                }}
                id="Username"
                label={usernameLabel}
                helperText={userNameHelperText}
                name="Username"
                onBlur={checkMultiFactor}
                onChange={handleChange}
                validators={userNameValidations}
                value={fields.Username}
                style={{
                    height: '20px',
                    margin: '15px 0 50px 0',
                }}
                fullWidth={true}
                errorMessages={userNameErrorMessages}
            />
            <TextValidator
                inputProps={{
                    tabIndex: 2,
                }}
                label={currentPasswordLabel}
                helperText={currentPasswordHelpblock}
                id="CurrentPassword"
                name="CurrentPassword"
                onChange={handleChange}
                type="password"
                validators={['required']}
                value={fields.CurrentPassword}
                style={{
                    height: '20px',
                    marginBottom: '50px',
                }}
                fullWidth={true}
                errorMessages={[fieldRequired]}
            />
            {usePasswordGeneration ? (
                <PasswordGenerator value={fields.NewPassword} setValue={setGenerated} />
            ) : (
                <>
                    <TextValidator
                        inputProps={{
                            tabIndex: 3,
                        }}
                        label={newPasswordLabel}
                        id="NewPassword"
                        name="NewPassword"
                        onChange={handleChange}
                        type="password"
                        validators={['required']}
                        value={fields.NewPassword}
                        style={{
                            height: '20px',
                            marginBottom: '30px',
                        }}
                        fullWidth={true}
                        errorMessages={[fieldRequired]}
                    />
                    {showPasswordMeter && <PasswordStrengthBar newPassword={fields.NewPassword} />}
                    <div
                        dangerouslySetInnerHTML={{ __html: newPasswordHelpblock }}
                        style={{ font: '12px Roboto,Helvetica, Arial, sans-serif', marginBottom: '15px' }}
                    />
                    <TextValidator
                        inputProps={{
                            tabIndex: 4,
                        }}
                        label={newPasswordVerifyLabel}
                        helperText={newPasswordVerifyHelpblock}
                        id="NewPasswordVerify"
                        name="NewPasswordVerify"
                        onChange={handleChange}
                        type="password"
                        validators={['required', `isPasswordMatch:${fields.NewPassword}`]}
                        value={fields.NewPasswordVerify}
                        style={{
                            height: '20px',
                            marginBottom: '50px',
                        }}
                        fullWidth={true}
                        errorMessages={[fieldRequired, passwordMatch]}
                    />
                </>
            )}
            {mfaOptions && (
                <>
                <FormControl>
                    <InputLabel id="multi-factor-authentication-selection">
                        {mfaSelectionLabel}
                    </InputLabel>
                    <Select
                        inputProps={{
                            tabIndex: 5
                        }}
                        labelId="multi-factor-authentication-selection"
                        id="MfaSelection"
                        name="MfaSelection"
                        value={fields.MfaSelection}
                        onChange={handleChange}
                    >
                        {mfaOptions.factors && mfaOptions.supportsPasscode && (
                            <MenuItem value="passcode">Passcode</MenuItem>
                        )}
                        {mfaOptions.factors.map((pair: any) => {
                            return <MenuItem value={pair.Value}>{pair.Key}</MenuItem>
                        })}
                    </Select>
                    <FormHelperText>{mfaSelectionHelpblock}</FormHelperText>
                </FormControl>
                {mfaOptions.supportsPasscode
                    && (!mfaOptions.factors || fields.MfaSelection === "passcode") && (
                    <TextValidator
                        inputProps={{
                            tabIndex: 6
                        }}
                        label={mfaPasscodeLabel}
                        helperText={mfaPasscodeHelpblock}
                        id="MfaPasscode"
                        name="MfaPasscode"
                        onChange={handleChange}
                        type="password"
                        validators={['required']}
                        value={fields.MfaPasscode}
                        style={{
                            height: '20px',
                            marginBottom: '50px',
                        }}
                        fullWidth={true}
                        errorMessages={[fieldRequired]}
                    />
                )}
                </>
            )}

            {recaptcha.siteKey && recaptcha.siteKey !== '' && (
                <ReCaptcha setToken={setReCaptchaToken} shouldReset={shouldResetRecaptcha} />
            )}
        </FormGroup>
    );
};
