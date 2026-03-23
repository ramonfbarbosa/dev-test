import { BaseService } from "./BaseService";
import { dateUtils } from "@/helpers/date";
import { Client } from "@/types/api/Client";
import { ClientImport, ClientImportErrors } from "@/types/api/ClientImport";
import { PagedList } from "@/types/api/PagedList";

export type ClientListParams = {
  documentNumber?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
};

class ClientService extends BaseService {
  constructor() {
    super("client");
  }

  private isClient(value: unknown): value is Client {
    return typeof value === "object" && value !== null && "birthDate" in value;
  }

  private normalizeClient(client: Client): Client {
    return {
      ...client,
      birthDate: dateUtils.toDisplay(client.birthDate),
    };
  }

  private serializeClient(client: Client): Client {
    return {
      ...client,
      birthDate: dateUtils.toApi(client.birthDate),
    };
  }

  async getAll(params?: ClientListParams): Promise<PagedList<Client>> {
    const result = await this.get<PagedList<Client>>("", params);
    return {
      ...result,
      values: result.values.map((client) => this.normalizeClient(client)),
    };
  }

  async importCsv(file: File): Promise<{ message?: string; Message?: string }> {
    const formData = new FormData();
    formData.append("file", file);

    return await this.post<FormData, { message?: string; Message?: string }>("import", formData);
  }

  async exportCsv(): Promise<void> {
    const response = await this._axios.get(`${this._controller}/export`, {
      responseType: "blob",
    });

    const disposition = response.headers["content-disposition"] ?? "";
    const match = disposition.match(/filename="?([^";\n]+)"?/);
    const fileName = match?.[1] ?? "clientes.csv";

    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  }

  async create<T = Client, TR = string>(client: T): Promise<TR> {
    const payload = this.isClient(client) ? (this.serializeClient(client) as T) : client;
    return await this.post<T, TR>("", payload);
  }

  async getById<T = Client>(id: string): Promise<T> {
    const client = await this.get<T>(id);
    return this.isClient(client) ? (this.normalizeClient(client) as T) : client;
  }

  async update<T = Client, TR = void>(id: string, client: T): Promise<TR> {
    const payload = this.isClient(client) ? (this.serializeClient(client) as T) : client;
    return await this.put<T, TR>(id, payload);
  }

  async getImports(params?: ClientListParams): Promise<PagedList<ClientImport>> {
    return await this.get<PagedList<ClientImport>>("imports", params);
  }

  async getImportErrors(importId: string): Promise<ClientImportErrors> {
    return await this.get<ClientImportErrors>(`imports/${importId}/errors`);
  }
}

export default new ClientService();
