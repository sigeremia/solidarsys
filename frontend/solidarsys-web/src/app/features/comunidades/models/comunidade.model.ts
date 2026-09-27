  export interface ComunidadeListItem {
  ativa: boolean;
  idComunidade: number;
  nome: string;
  regiao: string;
  descricao: string;
  totalBeneficiarios: number;
  dataCriacao: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

export interface CriarComunidadeRequest {
  nome: string;
  regiao: string;
  descricao: string;
}

export interface AtualizarComunidadeRequest {
  nome: string;
  regiao: string;
  descricao: string;
}
