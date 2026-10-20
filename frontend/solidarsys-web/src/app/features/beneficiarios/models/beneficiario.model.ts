export interface BeneficiarioListItem {
  idBeneficiario: number;
  nome: string;
  cpf: string;
  dataNascimento: string;
  endereco: string;
  idComunidade: number;
  comunidade: string;
  totalMembrosFamilia: number;
  ativo: boolean;
  dataCriacao: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

export interface ComunidadeOpcao {
  idComunidade: number;
  nome: string;
}

export interface CriarBeneficiarioRequest {
  nome: string;
  cpf: string;
  dataNascimento: string;
  endereco: string;
  idComunidade: number;
}

export type AtualizarBeneficiarioRequest = CriarBeneficiarioRequest;