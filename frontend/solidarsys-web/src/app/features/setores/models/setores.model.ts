  export interface SetoresListItem {
  ativo: boolean;
  idSetores: number;
  nome: string;
  descricao: string;
  dataCriacao: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

export interface CriarSetoresRequest {
  nome: string;
  descricao: string;
}

export interface AtualizarSetoresRequest {
  nome: string;
  descricao: string;
}
