import React, { Suspense } from "react";
import { Helmet } from "react-helmet-async";
import { Button, Card, Col, Row } from "react-bootstrap";
import { useNavigate, useParams } from "react-router-dom";
import { useSuspenseQuery } from "@tanstack/react-query";

import Loader from "@/components/Loader";
import { TextFormField } from "@/components/form/TextFormField/TextFormField";
import { TextFormFieldType } from "@/components/form/TextFormField/TextFormFieldType";
import { NAVIGATION_PATH } from "@/constants";
import { ReactQueryKeys } from "@/constants/ReactQueryKeys";
import { format } from "@/helpers/format";
import ClientService from "@/services/ClientService";
import { Client } from "@/types/api/Client";

function formatDocumentNumber(documentNumber?: string) {
  const digits = (documentNumber ?? "").replace(/\D/g, "");
  if (digits.length === 14) return format.toMask(digits, "##.###.###/####-##");
  if (digits.length === 11) return format.toMask(digits, "###.###.###-##");
  return documentNumber ?? "";
}

const ClientDetailsContent = () => {
  const navigate = useNavigate();
  const { id } = useParams();

  const { data } = useSuspenseQuery<Client>({
    queryKey: [ReactQueryKeys.CLIENT, "detail", id],
    meta: {
      fetchFn: async () => {
        if (!id) {
          throw new Error("Cliente não informado");
        }

        return await ClientService.getById(id);
      },
    },
  });

  return (
    <Card>
      <Card.Header>
        <Card.Title>Detalhes do Cliente</Card.Title>
      </Card.Header>
      <Card.Body>
        <Row>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="firstName"
              label="Nome"
              value={data.firstName}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="lastName"
              label="Sobrenome"
              value={data.lastName}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="birthDate"
              label="Data de nascimento"
              value={data.birthDate}
              disabled
            />
          </Col>
        </Row>
        <Row>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="email"
              label="Email"
              value={data.email}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="phoneNumber"
              label="Telefone"
              value={format.toMask(data.phoneNumber ?? "", "(##) #####-####")}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="documentNumber"
              label="Documento (CPF/CNPJ)"
              value={formatDocumentNumber(data.documentNumber)}
              disabled
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
              value={format.toMask(data.address.postalCode ?? "", "#####-###")}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="address.addressLine"
              label="Endereço"
              value={data.address.addressLine}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="address.number"
              label="Número"
              value={data.address.number}
              disabled
            />
          </Col>
        </Row>
        <Row>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="address.complement"
              label="Complemento"
              value={data.address.complement}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="address.neighborhood"
              label="Bairro"
              value={data.address.neighborhood}
              disabled
            />
          </Col>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="address.city"
              label="Cidade"
              value={data.address.city}
              disabled
            />
          </Col>
        </Row>
        <Row>
          <Col md={4}>
            <TextFormField
              componentType={TextFormFieldType.INPUT}
              name="address.state"
              label="Estado"
              value={data.address.state}
              disabled
            />
          </Col>
        </Row>
        <br />
        <Button variant="primary" onClick={() => navigate(`/clientes/edit/${id}`)}>
          Editar
        </Button>
        <Button
          variant="secondary"
          style={{ marginLeft: 5 }}
          onClick={() => navigate(NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE)}
        >
          Voltar
        </Button>
      </Card.Body>
    </Card>
  );
};

const ClientDetails = () => {
  return (
    <React.Fragment>
      <Helmet title="Detalhes do Cliente" />
      <Suspense fallback={<><Loader /><br /><br /></>}>
        <ClientDetailsContent />
      </Suspense>
    </React.Fragment>
  );
};

export default ClientDetails;
