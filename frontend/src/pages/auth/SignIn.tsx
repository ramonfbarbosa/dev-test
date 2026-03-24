import React, { useState } from "react";
import { Helmet } from "react-helmet-async";
import { Card, Alert, Button } from "react-bootstrap";
import { useNavigate, useSearchParams } from "react-router-dom";
import * as Yup from "yup";
import { Formik } from "formik";
import { Loader, Moon, Sun } from "react-feather";

import { login } from "@/redux/slices/auth.slice";
import useAppDispatch from "@/hooks/useAppDispatch";
import useTheme from "@/hooks/useTheme";
import { THEME } from "@/constants";
import AuthService from "@/services/AuthService";
import { TextFormField } from "@/components/form/TextFormField/TextFormField";
import { TextFormFieldType } from "@/components/form/TextFormField/TextFormFieldType";
import { getApiErrorDetails } from "@/utils/errorHandling";

function SignInPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { theme, setTheme } = useTheme();
  const isDark = theme === THEME.DARK;

  const [searchParams] = useSearchParams();
  const [redirectUri] = useState(searchParams.get("redirect_uri"));

  return (
    <React.Fragment>
      <Helmet title="Login" />
      <div className="text-center mt-4">
        <div className="login-brand mb-3">
          <svg width="30" height="30" viewBox="0 0 33 33" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect x="5" y="5" width="22" height="22" rx="4" fill="#4F46E5" transform="rotate(45 16 16)" />
          </svg>
          <span>ClientControl</span>
        </div>
        <h2>Bem vindo</h2>
        <p className="lead">Acesse com sua conta para continuar</p>
      </div>

      <Card>
        <Card.Body>
          <div className="m-sm-3">
            <Formik
              initialValues={{
                username: "",
                password: "",
                submit: false,
              }}
              validationSchema={Yup.object().shape({
                username: Yup.string()
                  .max(50, "Usuário deve ter no máximo 50 caracteres")
                  .required("Usuário é obrigatório"),
                password: Yup.string().max(255).required("Senha é obrigatória"),
              })}
                onSubmit={async (values, { setErrors }) => {
                  try {
                    const response = await AuthService.login(values.username, values.password);
                    dispatch(login(response))
                    navigate(redirectUri ?? "/");
                  } catch (error: any) {
                    const apiError = getApiErrorDetails(error);
                    const message = apiError.errors[0]?.value || apiError.message || "Usuário ou senha inválidos";
                    setErrors({ submit: message });
                  }
                }}
            >
              {({
                errors,
                handleBlur,
                handleChange,
                handleSubmit,
                isSubmitting,
                touched,
                values,
              }) => (
                <>
                  {errors.submit && <Alert variant="danger">{errors.submit}</Alert>}
                  <TextFormField
                    componentType={TextFormFieldType.INPUT}
                    name="username"
                    label="Usuário"
                    placeholder="Digite seu username"
                    value={values.username}
                    isInvalid={Boolean(touched.username && errors.username)}
                    onBlur={handleBlur}
                    handleChange={handleChange}
                    formikError={touched.username ? errors.username : undefined}
                    style={{ marginBottom: 10 }}
                  />
                  <TextFormField
                    componentType={TextFormFieldType.INPUT}
                    name="password"
                    password
                    label="Senha"
                    placeholder="Digite sua senha"
                    value={values.password}
                    isInvalid={Boolean(touched.password && errors.password)}
                    onBlur={handleBlur}
                    handleChange={handleChange}
                    formikError={touched.password ? errors.password : undefined}
                  />

                  <div className="d-grid gap-2 mt-3">
                    <Button
                      size="lg"
                      disabled={isSubmitting}
                      onClick={() => handleSubmit()}
                    >
                      {isSubmitting ? <Loader /> : "Login"}
                    </Button>
                  </div>
                </>
              )}
            </Formik>
          </div>
        </Card.Body>
      </Card>
      <div className="text-center mt-3">
        <a
          role="button"
          className="d-inline-flex align-items-center gap-1"
          onClick={() => setTheme(isDark ? THEME.DEFAULT : THEME.DARK)}
          title={isDark ? "Tema claro" : "Tema escuro"}
          style={{ cursor: "pointer", opacity: 0.7 }}
        >
          {isDark ? <Sun size={16} /> : <Moon size={16} />}
          <small>{isDark ? "Tema claro" : "Tema escuro"}</small>
        </a>
      </div>
    </React.Fragment >
  );
}

export default SignInPage;
