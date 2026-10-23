-- =============================================================================
-- Cria a tabela "usuario", utilizada na autenticação (LoginController).
-- Colunas espelham a entidade Oficina.Domain.Entitys.Usuario.
-- =============================================================================

CREATE TABLE IF NOT EXISTS usuario (
    id_usuario   SERIAL       PRIMARY KEY,
    login       VARCHAR(100) NOT NULL UNIQUE,
    senha       VARCHAR(255) NOT NULL,
    ativo       BOOLEAN      NOT NULL DEFAULT TRUE
);
