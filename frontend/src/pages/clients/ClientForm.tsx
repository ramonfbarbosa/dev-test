import { Helmet } from "react-helmet-async";
import { useNavigate, useParams } from "react-router-dom";
import { NAVIGATION_PATH } from "@/constants";
import { Client } from "@/types/api/Client";
import { TextFormFieldType } from "@/components/form/TextFormField/TextFormFieldType";
import { TextFormField } from "@/components/form/TextFormField/TextFormField";
import Loader from "@/components/Loader";
import { toastr } from "@/utils/toastr";
import { errorHandling, getApiErrorDetails, normalizeApiErrorFieldPath } from "@/utils/errorHandling";
import ClientService from "@/services/ClientService";
import ViaCepService from "@/services/ViaCepService";
import { dateUtils } from "@/helpers/date";
import { format } from "@/helpers/format";
import { useSuspenseQuery, useQueryClient } from "@tanstack/react-query";
import { ReactQueryKeys } from "@/constants/ReactQueryKeys";
import yup from "@/utils/yup";
import React, { Suspense } from "react";
import { Button, Card, Col, Form, Row } from "react-bootstrap";
import { Formik, FormikHelpers } from "formik";

const INITIAL_VALUES: Client = {
  firstName: "",
  lastName: "",
  phoneNumber: "",
  email: "",
  documentNumber: "",
  birthDate: "",
  address: {
    postalCode: "",
    addressLine: "",
    number: "",
    complement: "",
    neighborhood: "",
    city: "",
    state: "",
  },
};

const POSTAL_CODE_LENGTH = 8;

const normalizePostalCode = (postalCode: string) => postalCode.replace(/\D/g, "");

function formatDocumentValue(raw: string): string {
  const digits = (raw ?? "").replace(/\D/g, "").substring(0, 14);
  if (digits.length === 0) return "";
  const mask = digits.length > 11 ? "##.###.###/####-##" : "###.###.###-##";
  return format.toMask(digits, mask);
}

const schemaValidation = yup.object().shape({
  firstName: yup.string().required("Nome é obrigatório"),
  lastName: yup.string().required("Sobrenome é obrigatório"),
  phoneNumber: yup.string().required("Telefone é obrigatório"),
  email: yup.string().email("Email inválido").required("Email é obrigatório"),
  documentNumber: yup.string().required("Documento é obrigatório").test(
    "document-length",
    "Documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ)",
    (value) => {
      if (!value) return true;
      const digits = value.replace(/\D/g, "").length;
      return digits === 11 || digits === 14;
    },
  ),
  birthDate: yup.string().required("Data de nascimento é obrigatória").test("birthDate", "Data de nascimento inválida", (value) => !value || dateUtils.isDisplayDate(value)),
  address: yup.object().shape({
    postalCode: yup.string().required("CEP é obrigatório").test(
      "cep-length",
      "CEP deve conter exatamente 8 dígitos",
      (value) => !value || value.replace(/\D/g, "").length === 8,
    ),
    addressLine: yup.string().required("Endereço é obrigatório"),
    number: yup.string().required("Número é obrigatório"),
    neighborhood: yup.string().required("Bairro é obrigatório"),
    city: yup.string().required("Cidade é obrigatória"),
    state: yup.string().required("Estado é obrigatório"),
  }),
});

const ClientForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();

  const queryClient = useQueryClient();
  const postalCodeLookupAbortControllerRef = React.useRef<AbortController | null>(null);
  const lastResolvedPostalCodeRef = React.useRef("");
  const [isLoadingPostalCode, setIsLoadingPostalCode] = React.useState(false);
  const { data } = useSuspenseQuery<Client>({
    queryKey: [ReactQueryKeys.CLIENT, id],
    meta: {
      fetchFn: async () => {
        if (id) {
          return await ClientService.getById(id);
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
      queryClient.invalidateQueries({ queryKey: [ReactQueryKeys.CLIENT, id] });
    }
  }, [id, queryClient]);

  React.useEffect(() => () => {
    postalCodeLookupAbortControllerRef.current?.abort();
  }, []);

  const fillAddressByPostalCode = React.useCallback(async (
    postalCode: string,
    currentAddress: Client["address"],
    setFieldValue: FormikHelpers<Client>["setFieldValue"],
    setFieldError: FormikHelpers<Client>["setFieldError"],
  ) => {
    const normalizedPostalCode = normalizePostalCode(postalCode);

    if (normalizedPostalCode.length !== POSTAL_CODE_LENGTH || normalizedPostalCode === lastResolvedPostalCodeRef.current) {
      return;
    }

    postalCodeLookupAbortControllerRef.current?.abort();

    const abortController = new AbortController();
    postalCodeLookupAbortControllerRef.current = abortController;
    setIsLoadingPostalCode(true);

    try {
      const addressLookup = await ViaCepService.getAddressByPostalCode(normalizedPostalCode, abortController.signal);

      if (postalCodeLookupAbortControllerRef.current !== abortController) {
        return;
      }

      lastResolvedPostalCodeRef.current = normalizedPostalCode;
      setFieldError("address.postalCode", undefined);

      await setFieldValue("address.addressLine", addressLookup.addressLine || currentAddress.addressLine, false);
      await setFieldValue("address.neighborhood", addressLookup.neighborhood || currentAddress.neighborhood, false);
      await setFieldValue("address.city", addressLookup.city || currentAddress.city, false);
      await setFieldValue("address.state", addressLookup.state || currentAddress.state, false);
    } catch (error) {
      if (abortController.signal.aborted) {
        return;
      }

      lastResolvedPostalCodeRef.current = "";
      const message = error instanceof Error ? error.message : "Não foi possível buscar o endereço pelo CEP.";
      setFieldError("address.postalCode", message);
      toastr({ title: message, icon: "error" });
    } finally {
      if (postalCodeLookupAbortControllerRef.current === abortController) {
        setIsLoadingPostalCode(false);
      }
    }
  }, []);

  async function onSubmit(values: Client, { setFieldError }: FormikHelpers<Client>) {
    try {
      const clientToSave: Client = {
        ...values,
        phoneNumber: values.phoneNumber.replace(/\D/g, ''),
        address: {
          ...values.address,
          postalCode: values.address.postalCode.replace(/\D/g, ''),
        },
      };
      if (id) {
        await ClientService.update(id, clientToSave);
        toastr({ title: "Cliente atualizado com sucesso", icon: "success" });
      } else {
        await ClientService.create(clientToSave);
        toastr({ title: "Cliente criado com sucesso", icon: "success" });
      }
      queryClient.removeQueries({ queryKey: [ReactQueryKeys.CLIENT, "listing"] });
      navigate(NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE);
    } catch (err: any) {
      const apiError = getApiErrorDetails(err);
      const fieldErrors = apiError.errors.filter(item => item.key && item.value);

      fieldErrors.forEach(item => {
        setFieldError(normalizeApiErrorFieldPath(item.key), item.value);
      });

      if (fieldErrors.length === 0 || apiError.errors.some(item => !item.key)) {
        errorHandling(err, apiError.message, "Erro");
      }
    }
  }

  const title = id ? "Editar Cliente" : "Novo Cliente";

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
              enableReinitialize={true}
            >
              {({
                handleSubmit,
                handleChange,
                handleBlur,
                errors,
                values,
                setFieldValue,
                setFieldError,
                isSubmitting,
                submitCount,
              }) => {
                const showValidationErrors = submitCount > 0;
                const getFieldError = (error?: string) => showValidationErrors ? error : undefined;
                const handleDocumentChange = (event: { target: { name: string; value: string } }) => {
                  const formatted = formatDocumentValue(event.target.value);
                  setFieldValue("documentNumber", formatted);
                };

                const handlePostalCodeChange = (event: { target: { name: string; value: string } }) => {
                  handleChange(event);

                  const normalizedPostalCode = normalizePostalCode(event.target.value);

                  if (normalizedPostalCode.length < POSTAL_CODE_LENGTH) {
                    postalCodeLookupAbortControllerRef.current?.abort();
                    lastResolvedPostalCodeRef.current = "";
                    setIsLoadingPostalCode(false);
                    setFieldError("address.postalCode", undefined);
                    void setFieldValue("address.addressLine", "", false);
                    void setFieldValue("address.neighborhood", "", false);
                    void setFieldValue("address.city", "", false);
                    void setFieldValue("address.state", "", false);
                    return;
                  }

                  void fillAddressByPostalCode(event.target.value, values.address, setFieldValue, setFieldError);
                };

                const handlePostalCodeBlur = (event: React.FocusEvent<HTMLInputElement>) => {
                  handleBlur(event);
                  void fillAddressByPostalCode(event.target.value, values.address, setFieldValue, setFieldError);
                };
 
                return (
                  <Form noValidate onSubmit={handleSubmit}>
                  <Row>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="firstName"
                        label="Nome"
                        required
                        placeholder="Nome"
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.firstName}
                        formikError={getFieldError(errors.firstName)}
                      />
                    </Col>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="lastName"
                        label="Sobrenome"
                        required
                        placeholder="Sobrenome"
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.lastName}
                        formikError={getFieldError(errors.lastName)}
                      />
                    </Col>
                    <Col md={4}>
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
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="phoneNumber"
                        label="Telefone"
                        required
                        placeholder="Telefone"
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.phoneNumber}
                        formikError={getFieldError(errors.phoneNumber)}
                        mask="(##) #####-####"
                      />
                    </Col>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="documentNumber"
                        label="Documento (CPF/CNPJ)"
                        required
                        placeholder="Documento"
                        handleBlur={handleBlur}
                        handleChange={handleDocumentChange}
                        value={values.documentNumber}
                        format={formatDocumentValue}
                        formikError={getFieldError(errors.documentNumber)}
                      />
                    </Col>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.DATE_PICKER}
                        name="birthDate"
                        label="Data de nascimento"
                        required
                        placeholderText="Data de nascimento"
                        handleChange={handleChange}
                        value={values.birthDate}
                        formikError={getFieldError(errors.birthDate)}
                        maxDate={new Date()}
                        showYearDropdown
                        yearDropdownItemNumber={100}
                      />
                    </Col>
                  </Row>
                  <br />
                  <Row>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="address.postalCode"
                        label="CEP"
                        required
                        placeholder={isLoadingPostalCode ? "Buscando CEP..." : "CEP"}
                        handleBlur={handleBlur}
                        handleChange={handlePostalCodeChange}
                        onBlur={handlePostalCodeBlur}
                        value={values.address.postalCode}
                        formikError={getFieldError(errors.address?.postalCode)}
                        mask="#####-###"
                        tooltip="Ao informar um CEP valido, o endereco sera preenchido automaticamente."
                      />
                    </Col>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="address.addressLine"
                        label="Endereço"
                        required
                        placeholder="Preenchido pelo CEP"
                        disabled
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.address.addressLine}
                        formikError={getFieldError(errors.address?.addressLine)}
                      />
                    </Col>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="address.number"
                        label="Número"
                        required
                        placeholder="Número"
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.address.number}
                        formikError={getFieldError(errors.address?.number)}
                      />
                    </Col>
                  </Row>
                  <Row>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="address.complement"
                        label="Complemento"
                        placeholder="Complemento"
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.address.complement}
                        formikError={getFieldError(errors.address?.complement)}
                      />
                    </Col>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="address.neighborhood"
                        label="Bairro"
                        required
                        placeholder="Preenchido pelo CEP"
                        disabled
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.address.neighborhood}
                        formikError={getFieldError(errors.address?.neighborhood)}
                      />
                    </Col>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="address.city"
                        label="Cidade"
                        required
                        placeholder="Preenchido pelo CEP"
                        disabled
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.address.city}
                        formikError={getFieldError(errors.address?.city)}
                      />
                    </Col>
                  </Row>
                  <Row>
                    <Col md={4}>
                      <TextFormField
                        componentType={TextFormFieldType.INPUT}
                        name="address.state"
                        label="Estado"
                        required
                        placeholder="Preenchido pelo CEP"
                        disabled
                        handleBlur={handleBlur}
                        handleChange={handleChange}
                        value={values.address.state}
                        formikError={getFieldError(errors.address?.state)}
                      />
                    </Col>
                  </Row>
                  <br />
                  <Button type="submit" variant="primary" disabled={isSubmitting}>
                    {isSubmitting ? "Salvando..." : "Salvar"}
                  </Button>
                  <Button
                    variant="secondary"
                    style={{ marginLeft: 5 }}
                    onClick={() => navigate(NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE)}
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

export default ClientForm;
