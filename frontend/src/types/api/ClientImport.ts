export type ClientImport = {
  id: string;
  originalFileName: string;
  status: number;
  statusText: string;
  uploadedByUserName: string;
  totalRows: number;
  importedRows: number;
  failureCount: number;
  errorMessage?: string;
  createdAt: string;
  startedAt?: string;
  finishedAt?: string;
};

export type ImportErrorDetail = {
  lineNumber: number;
  message: string;
};

export type ClientImportErrors = {
  originalFileName: string;
  statusText: string;
  totalRows: number;
  importedRows: number;
  failureCount: number;
  errors: ImportErrorDetail[];
};
