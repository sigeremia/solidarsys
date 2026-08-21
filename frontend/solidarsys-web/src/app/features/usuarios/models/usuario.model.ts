import { NivelAcesso } from '../../../core/models/auth.model';

export interface UsuarioListItem {
  id: number;
  nome: string;
  email: string;
  nivelAcesso: NivelAcesso;
  setor: string | null;
  ativo: boolean;
  dataCriacao: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

export interface CriarUsuarioRequest {
  nome: string;
  email: string;
  senha: string;
  dataNascimento: string;
  nivelAcesso: NivelAcesso;
  idSetor: number | null;
  cpf: string;
}

export interface AtualizarUsuarioRequest {
  nome: string;
  dataNascimento: string;
  nivelAcesso: NivelAcesso;
  idSetor: number | null;
}
