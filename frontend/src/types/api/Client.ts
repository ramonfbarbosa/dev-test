import { Address } from "./Address";
import { BaseEntity } from "./BaseEntity";

export type Client = {
  id?: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  documentNumber: string;
  birthDate: string;
  address: Address;
} & BaseEntity;
