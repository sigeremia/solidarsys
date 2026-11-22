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

CREATE TABLE IF NOT EXISTS comunidades (
    id_comunidade      SERIAL PRIMARY KEY,
    nome               VARCHAR(150) NOT NULL,
    regiao             VARCHAR(150) NOT NULL,
    descricao          TEXT,
    data_criacao       TIMESTAMPTZ NOT NULL,
    ativa              BOOLEAN NOT NULL DEFAULT TRUE,
    total_beneficiarios INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS beneficiarios (
    id_beneficiario      SERIAL PRIMARY KEY,
    nome                 VARCHAR(150) NOT NULL,
    cpf                  CHAR(11) NOT NULL,
    data_nascimento      DATE NOT NULL,
    endereco             VARCHAR(255) NOT NULL,
    latitude             DOUBLE PRECISION,
    longitude            DOUBLE PRECISION,
    id_comunidade        INT NOT NULL REFERENCES comunidades(id_comunidade),
    total_membros_familia INTEGER NOT NULL DEFAULT 0,
    ativo                BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_beneficiarios_cpf UNIQUE (cpf)
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

-- Comunidade e beneficiários iniciais de Caxias do Sul / RS
INSERT INTO comunidades (nome, regiao, descricao, data_criacao, ativa, total_beneficiarios)
SELECT
    'Comunidade Esperança',
    'Caxias do Sul / RS',
    'Apoio a famílias em vulnerabilidade com cestas básicas e atendimento social.',
    '2024-01-12 00:00:00+00',
    TRUE,
    8
WHERE NOT EXISTS (SELECT 1 FROM comunidades WHERE nome = 'Comunidade Esperança');

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'Maria Silva', '12345678909', '1986-04-08',
       'Rua Marechal Floriano, 120, Caxias do Sul - RS', -29.1629373, -51.1843351,
       id_comunidade, 3, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'João Santos', '98765432100', '1978-07-14',
       'Rua Ludovico Cavinatto, 44, Caxias do Sul - RS', -29.1450474, -51.1971040,
       id_comunidade, 2, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'Carlos Souza', '45678912355', '1989-11-20',
       'Rua Sinimbu, 320, Caxias do Sul - RS', -29.1687605, -51.1759491,
       id_comunidade, 4, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'Ana Oliveira', '52998224725', '1991-02-12',
       'Rua Moreira César, 85, Caxias do Sul - RS', -29.1664518, -51.1855844,
       id_comunidade, 3, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'Pedro Lima', '93541134780', '1983-08-21',
       'Avenida Júlio de Castilhos, 240, Caxias do Sul - RS', -29.1677185, -51.1717769,
       id_comunidade, 4, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'Luciana Ferreira', '11144477735', '1975-06-03',
       'Rua Pinheiro Machado, 17, Caxias do Sul - RS', -29.1670341, -51.1904197,
       id_comunidade, 2, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'Rafael Costa', '39053344705', '1994-12-16',
       'Rua Plácido de Castro, 62, Caxias do Sul - RS', -29.1717178, -51.1663002,
       id_comunidade, 5, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;

INSERT INTO beneficiarios (
    nome, cpf, data_nascimento, endereco, latitude, longitude,
    id_comunidade, total_membros_familia, ativo, data_criacao
)
SELECT 'Beatriz Almeida', '16899535009', '1988-03-29',
       'Rua Garibaldi, 311, Caxias do Sul - RS', -29.1647308, -51.1828916,
       id_comunidade, 3, TRUE, NOW()
FROM comunidades WHERE nome = 'Comunidade Esperança'
ON CONFLICT (cpf) DO UPDATE SET
    endereco = EXCLUDED.endereco,
    latitude = EXCLUDED.latitude,
    longitude = EXCLUDED.longitude;
