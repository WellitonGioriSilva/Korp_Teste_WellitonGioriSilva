import { StatusNotaFiscal } from "./nota-fiscal.model";

export interface ImpressaoNotaEvent {
  notaFiscalId: number;
  status: StatusNotaFiscal;
  message?: string;
}