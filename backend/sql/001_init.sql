-- SolidarSYS — Migration inicial
-- Executar no PostgreSQL 16 como superusuário ou usuário do banco

CREATE TYPE nivel_acesso AS ENUM ('ADMINISTRADOR', 'GESTOR', 'OPERADOR', 'VOLUNTARIO');

CREATE TABLE IF NOT EXISTS setores (
    id           SERIAL PRIMARY KEY,
    nome         VARCHAR(150) NOT NULL,
    descricao    TEXT,
    ativo        BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS permissoes (
    id        SERIAL PRIMARY KEY,
    nome      VARCHAR(150) NOT NULL,
    descricao TEXT,
    recurso   VARCHAR(100) NOT NULL,
    acao      VARCHAR(50)  NOT NULL
);

CREATE TABLE IF NOT EXISTS setor_permissoes (
    id_setor     INT NOT NULL REFERENCES setores(id) ON DELETE CASCADE,
    id_permissao INT NOT NULL REFERENCES permissoes(id) ON DELETE CASCADE,
    PRIMARY KEY (id_setor, id_permissao)
);

CREATE TABLE IF NOT EXISTS usuarios (
    id_usuario         SERIAL PRIMARY KEY,
    nome               VARCHAR(150) NOT NULL,
    email              VARCHAR(150) NOT NULL,
    senha              VARCHAR(255) NOT NULL,
    data_nascimento    DATE NOT NULL,
    nivel_acesso       nivel_acesso NOT NULL,
    id_setor           INT REFERENCES setores(id),
    cpf                CHAR(11) NOT NULL,
    ativo              BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao       TIMESTAMP NOT NULL DEFAULT NOW(),
    data_ultimo_acesso TIMESTAMP,
    tentativas_falhas  SMALLINT NOT NULL DEFAULT 0,
    bloqueado_ate      TIMESTAMP,
    CONSTRAINT uq_usuarios_email UNIQUE (email),
    CONSTRAINT uq_usuarios_cpf   UNIQUE (cpf)
);

CREATE TABLE IF NOT EXISTS refresh_tokens (
    id          SERIAL PRIMARY KEY,
    id_usuario  INT NOT NULL REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
    token       VARCHAR(500) NOT NULL UNIQUE,
    expira_em   TIMESTAMP NOT NULL,
    revogado    BOOLEAN NOT NULL DEFAULT FALSE,
    criado_em   TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS tokens_recuperacao_senha (
    id          SERIAL PRIMARY KEY,
    id_usuario  INT NOT NULL REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
    token       VARCHAR(255) NOT NULL UNIQUE,
    expira_em   TIMESTAMP NOT NULL,
    usado       BOOLEAN NOT NULL DEFAULT FALSE,
    criado_em   TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS doadores (
    id_doador     SERIAL PRIMARY KEY,
    tipo          VARCHAR(20) NOT NULL,
    nome          VARCHAR(100) NOT NULL,
    nome_fantasia VARCHAR(100),
    documento     VARCHAR(18) NOT NULL,
    telefone      VARCHAR(15),
    email         VARCHAR(150),
    data_cadastro DATE NOT NULL DEFAULT CURRENT_DATE,
    observacao    TEXT,
    ativo         BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_doadores_documento UNIQUE (documento)
);

CREATE TABLE IF NOT EXISTS doacoes (
    id            SERIAL PRIMARY KEY,
    id_doador     INT NOT NULL REFERENCES doadores(id_doador),
    data          DATE NOT NULL,
    valor         NUMERIC(12, 2) NOT NULL,
    descricao     VARCHAR(255) NOT NULL,
    tipo          VARCHAR(20) NOT NULL,
    data_cadastro TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_usuarios_email      ON usuarios(email);
CREATE INDEX IF NOT EXISTS idx_usuarios_cpf        ON usuarios(cpf);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_uid  ON refresh_tokens(id_usuario);
CREATE INDEX IF NOT EXISTS idx_tokens_rec_uid      ON tokens_recuperacao_senha(id_usuario);
CREATE INDEX IF NOT EXISTS idx_doacoes_doador_data ON doacoes(id_doador, data);

-- Setor inicial e usuário ADMINISTRADOR padrão
-- IMPORTANTE: altere a senha abaixo antes de usar em produção.
-- Hash BCrypt de "Admin@123" (cost 12):
INSERT INTO setores (nome, descricao) VALUES ('Administrativo', 'Setor administrativo principal') ON CONFLICT DO NOTHING;

INSERT INTO usuarios (nome, email, senha, data_nascimento, nivel_acesso, id_setor, cpf, ativo)
SELECT
    'Administrador',
    'admin@solidarsys.org',
    '$2a$12$LXAhGAq3F8sGHc0yXX5lrOzXJGkbPHl0AZRZ.BeFgvGD8yMvxCc4i',  -- Admin@123
    '1990-01-01',
    'ADMINISTRADOR',
    (SELECT id FROM setores WHERE nome = 'Administrativo' LIMIT 1),
    '00000000000',
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM usuarios WHERE email = 'admin@solidarsys.org');
