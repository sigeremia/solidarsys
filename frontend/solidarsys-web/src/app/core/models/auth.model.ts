export type NivelAcesso = 'ADMINISTRADOR' | 'GESTOR' | 'OPERADOR' | 'VOLUNTARIO';

export interface UsuarioInfo {
  id: number;
  nome: string;
  email: string;
  nivelAcesso: NivelAcesso;
  setor: string | null;
}

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  accessToken: string;
  usuario: UsuarioInfo;
}

export interface EsqueceuSenhaRequest {
  email: string;
}

export interface RedefinirSenhaRequest {
  token: string;
  novaSenha: string;
  confirmarSenha: string;
}
