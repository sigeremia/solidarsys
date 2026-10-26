export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

export interface VisitaListItem {
  idVisita: number;
  idBeneficiario: number;
  beneficiario: string;
  dataVisita: string;
  responsavel: string;
  observacoes?: string | null;
  dataCadastro: string;
}

export interface BeneficiarioOpcao {
  idBeneficiario: number;
  nome: string;
}

export interface CriarVisitaRequest {
  idBeneficiario: number;
  dataVisita: string;
  responsavel: string;
  observacoes?: string | null;
}

export type AtualizarVisitaRequest = CriarVisitaRequest;
