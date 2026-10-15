-- =============================================================================
-- Script de exemplo: cria a tabela "exemplo" e insere dados iniciais.
-- Executado apenas na primeira inicialização do banco (volume vazio),
-- no banco POSTGRES_DB com o usuário POSTGRES_USER.
-- Prefira comandos idempotentes (IF NOT EXISTS / ON CONFLICT).
-- =============================================================================

-- Criação de tabela
CREATE TABLE IF NOT EXISTS exemplo (
    id          SERIAL PRIMARY KEY,
    nome        VARCHAR(100) NOT NULL UNIQUE,
    ativo       BOOLEAN      NOT NULL DEFAULT TRUE,
    criado_em   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Carga inicial de dados
INSERT INTO exemplo (nome) VALUES
    ('Registro 1'),
    ('Registro 2')
ON CONFLICT (nome) DO NOTHING;
