import axios from "axios";
import { Address } from "@/types/api/Address";

type ViaCepResponse = {
  cep: string;
  logradouro: string;
  bairro: string;
  localidade: string;
  uf: string;
  erro?: boolean;
};

type PostalCodeAddressLookup = Pick<Address, "addressLine" | "neighborhood" | "city" | "state">;

class ViaCepService {
  private readonly _http = axios.create({
    baseURL: "https://viacep.com.br/ws/",
  });

  async getAddressByPostalCode(postalCode: string, signal?: AbortSignal): Promise<PostalCodeAddressLookup> {
    const normalizedPostalCode = postalCode.replace(/\D/g, "");

    if (!/^\d{8}$/.test(normalizedPostalCode)) {
      throw new Error("Informe um CEP válido.");
    }

    try {
      const response = await this._http.get<ViaCepResponse>(`${normalizedPostalCode}/json/`, { signal });

      if (response.data.erro) {
        throw new Error("CEP não encontrado.");
      }

      return {
        addressLine: response.data.logradouro ?? "",
        neighborhood: response.data.bairro ?? "",
        city: response.data.localidade ?? "",
        state: response.data.uf ?? "",
      };
    } catch (error) {
      if (axios.isCancel(error)) {
        throw error;
      }

      if (error instanceof Error && (error.message === "Informe um CEP válido." || error.message === "CEP não encontrado.")) {
        throw error;
      }

      throw new Error("Não foi possível buscar o endereço pelo CEP.");
    }
  }
}

export default new ViaCepService();
