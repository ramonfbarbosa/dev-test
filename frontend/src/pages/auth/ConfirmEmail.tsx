import React from "react";
import { Helmet } from "react-helmet-async";
import { Alert, Button, Card, Spinner } from "react-bootstrap";
import { Link, useSearchParams } from "react-router-dom";
import { CheckCircle, AlertCircle, Moon, Sun } from "react-feather";
import { NAVIGATION_PATH, THEME } from "@/constants";
import useTheme from "@/hooks/useTheme";
import UserService from "@/services/UserService";

const ConfirmEmail = () => {
  const [searchParams] = useSearchParams();
  const [message, setMessage] = React.useState("Confirmando o email...");
  const [status, setStatus] = React.useState<"loading" | "success" | "error">("loading");
  const { theme, setTheme } = useTheme();
  const isDark = theme === THEME.DARK;

  const userId = searchParams.get("userId") ?? "";
  const token = searchParams.get("token") ?? "";

  React.useEffect(() => {
    let mounted = true;

    async function confirmEmail() {
      if (!userId || !token) {
        if (!mounted) {
          return;
        }

        setStatus("error");
        setMessage("O link de confirmação é inválido.");
        return;
      }

      try {
        const response = await UserService.confirmEmail(userId, token);

        if (!mounted) {
          return;
        }

        setStatus("success");
        setMessage(response.message ?? response.Message ?? "Email confirmado com sucesso.");
      } catch (error: any) {
        if (!mounted) {
          return;
        }

        setStatus("error");
        setMessage(error.message ?? "Não foi possível confirmar o email.");
      }
    }

    confirmEmail();

    return () => {
      mounted = false;
    };
  }, [token, userId]);

  return (
    <React.Fragment>
      <Helmet title="Confirmar email" />
      <div className="text-center mt-4">
        <div className="login-brand mb-3">
          <svg width="30" height="30" viewBox="0 0 33 33" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect x="5" y="5" width="22" height="22" rx="4" fill="#4F46E5" transform="rotate(45 16 16)" />
          </svg>
          <span>ClientControl</span>
        </div>
        <h2>Confirmação de email</h2>
        <p className="lead">Verificando seu endereço de email</p>
      </div>

      <Card>
        <Card.Body>
          <div className="m-sm-3">
            {status === "loading" && (
              <div className="text-center py-4">
                <Spinner animation="border" className="mb-3" />
                <p className="mb-0">{message}</p>
              </div>
            )}

            {status === "success" && (
              <div className="text-center py-3">
                <CheckCircle size={48} className="text-success mb-3" />
                <Alert variant="success">{message}</Alert>
              </div>
            )}

            {status === "error" && (
              <div className="text-center py-3">
                <AlertCircle size={48} className="text-danger mb-3" />
                <Alert variant="danger">{message}</Alert>
              </div>
            )}

            <div className="d-grid gap-2 mt-3">
              <Link to={NAVIGATION_PATH.AUTH.SIGN_IN.ABSOLUTE}>
                <Button size="lg" variant="primary" className="w-100">
                  Ir para o login
                </Button>
              </Link>
            </div>
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
    </React.Fragment>
  );
};

export default ConfirmEmail;
