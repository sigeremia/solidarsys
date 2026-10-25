export type TipoDoador = 'PESSOA_FISICA' | 'PESSOA_JURIDICA';

export interface DoadorListItem {
  idDoador: number;
  tipo: TipoDoador;
  nome: string;
  nomeFantasia: string | null;
  documento: string;
  telefone: string | null;
  email: string | null;
  dataCadastro: string;
  observacao: string | null;
  ativo: boolean;
  ultimaDoacao: string | null;
}

export interface DoadorRequest {
  tipo: TipoDoador;
  nome: string;
  nomeFantasia: string | null;
  documento: string;
  telefone: string | null;
  email: string | null;
  observacao: string | null;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}