import React, { Suspense } from "react";
import { Helmet } from "react-helmet-async";
import { useNavigate, useParams } from "react-router-dom";
import { useSuspenseQuery, useQueryClient } from "@tanstack/react-query";
import { Button, Card, Col, Form, Row } from "react-bootstrap";
import { Formik, FormikHelpers } from "formik";
import Loader from "@/components/Loader";
import yup from "@/utils/yup";
import { NAVIGATION_PATH } from "@/constants";
import { ReactQueryKeys } from "@/constants/ReactQueryKeys";
import UserService from "@/services/UserService";
import { User } from "@/types/api/User";
import { TextFormField } from "@/components/form/TextFormField/TextFormField";
import { TextFormFieldType } from "@/components/form/TextFormField/TextFormFieldType";
import { UserProfile, userProfileOptions } from "@/types/api/enums/UserProfile";
import { toastr } from "@/utils/toastr";
import { errorHandling, getApiErrorDetails, normalizeApiErrorFieldPath } from "@/utils/errorHandling";

const INITIAL_VALUES: User = {
  username: "",
  email: "",
  emailConfirmed: false,
  active: true,
  password: "",
  profile: UserProfile.Administrator,
};

const UserForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const queryClient = useQueryClient();

  const schemaValidation = React.useMemo(() => yup.object().shape({
    username: yup.string().max(50, "Usuário deve ter no máximo 50 caracteres").required("Usuário é obrigatório"),
    email: yup.string().max(255, "Email deve ter no máximo 255 caracteres").email("Email informado é inválido").required("Email é obrigatório"),
    password: id
      ? yup.string().notRequired()
      : yup.string().min(6, "Senha deve ter pelo menos 6 caracteres").required("Senha é obrigatória"),
    profile: yup
      .mixed<UserProfile>()
      .oneOf(Object.values(UserProfile).filter((value) => typeof value === "number") as UserProfile[], "Perfil inválido")
      .required("Perfil é obrigatório"),
  }), [id]);

  const { data } = useSuspenseQuery<User>({
    queryKey: [ReactQueryKeys.USER, id ?? "new"],
    meta: {
      fetchFn: async () => {
        if (id) {
          return await UserService.getById(id);
        }

        return INITIAL_VALUES;
      },
    },
    refetchOnMount: true,
    refetchOnWindowFocus: true,
    refetchOnReconnect: true,
  });

  React.useEffect(() => {
    if (id) {
      queryClient.invalidateQueries({ queryKey: [ReactQueryKeys.USER, id] });
    }
  }, [id, queryClient]);

  async function onSubmit(values: User, { setFieldError }: FormikHelpers<User>) {
    try {
      const shouldNotifyEmailConfirmation = !id || data.email !== values.email;

      if (id) {
        await UserService.update(id, values);
        await toastr({
          title: "Usuário atualizado com sucesso",
          text: shouldNotifyEmailConfirmation ? "Um novo email de confirmação foi enviado." : undefined,
          icon: "success",
          toast: !shouldNotifyEmailConfirmation,
          showConfirmButton: shouldNotifyEmailConfirmation,
          position: shouldNotifyEmailConfirmation ? "center" : "top-end",
        });
      } else {
        await UserService.create(values);
        await toastr({
          title: "Usuário criado com sucesso",
          text: "Um email de confirmação foi enviado para o usuário.",
          icon: "success",
          toast: false,
          showConfirmButton: true,
          position: "center",
        });
      }

      queryClient.removeQueries({ queryKey: [ReactQueryKeys.USER, "listing"] });
      navigate(NAVIGATION_PATH.USERS.LISTING.ABSOLUTE);
    } catch (error) {
      const apiError = getApiErrorDetails(error);
      const fieldErrors = apiError.errors.filter(item => item.key && item.value);

      fieldErrors.forEach(item => {
        setFieldError(normalizeApiErrorFieldPath(item.key), item.value);
      });

      if (fieldErrors.length === 0 || apiError.errors.some(item => !item.key)) {
        errorHandling(error, apiError.message, "Erro");
      }
    }
  }

  const title = id ? "Editar Usuário" : "Novo Usuário";

  return (
    <React.Fragment>
      <Helmet title={title} />
      <Suspense fallback={<><Loader /><br /><br /></>}>
        <Card>
          <Card.Header>
            <Card.Title>{title}</Card.Title>
          </Card.Header>
          <Card.Body>
            <Formik
              initialValues={data}
              validationSchema={schemaValidation}
              onSubmit={onSubmit}
              enableReinitialize
            >
              {({
                handleSubmit,
                handleChange,
                handleBlur,
                errors,
                values,
                isSubmitting,
                submitCount,
              }) => {
                const showValidationErrors = submitCount > 0;
                const getFieldError = (error?: string) => showValidationErrors ? error : undefined;

                return (
                  <Form noValidate onSubmit={handleSubmit}>
                    <Row>
                      <Col md={6}>
                        <TextFormField
                          componentType={TextFormFieldType.INPUT}
                          name="username"
                          label="Usuário"
                          required
                          placeholder="Usuário"
                          handleBlur={handleBlur}
                          handleChange={handleChange}
                          value={values.username}
                          formikError={getFieldError(errors.username)}
                        />
                      </Col>
                      <Col md={6}>
                        <TextFormField
                          componentType={TextFormFieldType.INPUT}
                          name="email"
                          label="Email"
                          required
                          placeholder="Email"
                          handleBlur={handleBlur}
                          handleChange={handleChange}
                          value={values.email}
                          formikError={getFieldError(errors.email)}
                        />
                      </Col>
                    </Row>
                    <Row>
                      <Col md={6}>
                        <TextFormField
                          componentType={TextFormFieldType.SELECT}
                          name="profile"
                          label="Perfil"
                          required
                          placeholder="Selecione o perfil"
                          handleChange={handleChange}
                          value={values.profile}
                          options={userProfileOptions()}
                          formikError={getFieldError(errors.profile as string | undefined)}
                        />
                      </Col>
                      {id && (
                        <Col md={6}>
                          <Form.Group>
                            <Form.Label>Status do email</Form.Label>
                            <Form.Control
                              value={values.emailConfirmed ? "Confirmado" : "Pendente"}
                              disabled
                            />
                          </Form.Group>
                        </Col>
                      )}
                    </Row>
                    {id && (
                      <Row>
                        <Col md={6}>
                          <Form.Group>
                            <Form.Label>Status do acesso</Form.Label>
                            <Form.Control
                              value={values.active ? "Ativo" : "Inativo"}
                              disabled
                            />
                          </Form.Group>
                        </Col>
                      </Row>
                    )}
                    {!id && (
                      <Row>
                        <Col md={6}>
                          <TextFormField
                            componentType={TextFormFieldType.INPUT}
                            name="password"
                            label="Senha"
                            required
                            placeholder="Senha"
                            handleBlur={handleBlur}
                            handleChange={handleChange}
                            value={values.password}
                            formikError={getFieldError(errors.password)}
                            password
                            disableAutoComplete
                          />
                        </Col>
                      </Row>
                    )}
                    {id && (
                      <Row>
                        <Col md={12}>
                          <small className="text-muted">
                            Ao alterar o email do usuário, uma nova confirmação será enviada e o status voltará para pendente.
                          </small>
                          {!values.active && (
                            <>
                              <br />
                              <small className="text-muted">
                                Usuários desativados não conseguem mais fazer login e serão deslogados na próxima ação no sistema.
                              </small>
                            </>
                          )}
                        </Col>
                      </Row>
                    )}
                    <br />
                    <Button type="submit" variant="primary" disabled={isSubmitting}>
                      {isSubmitting ? "Salvando..." : "Salvar"}
                    </Button>
                    <Button
                      variant="secondary"
                      style={{ marginLeft: 5 }}
                      onClick={() => navigate(NAVIGATION_PATH.USERS.LISTING.ABSOLUTE)}
                    >
                      Voltar
                    </Button>
                  </Form>
                );
              }}
            </Formik>
          </Card.Body>
        </Card>
      </Suspense>
    </React.Fragment>
  );
};

export default UserForm;
