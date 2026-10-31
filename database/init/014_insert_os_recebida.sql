-- OS completa para testes locais do fluxo de orçamento:
-- veículo FFF2026 (Gol do cliente Lucas Ferreira), com serviços e insumos,
-- histórico Recebida

--OS STATUS
INSERT INTO os_status
    (id_os, id_status, id_colaborador)
VALUES
    (1, 1, 2);

--OS COLABORADOR
INSERT INTO os_colaborador
    (id_os, id_colaborador)
VALUES
    (1, 2);

    DO $$
DECLARE
    v_id_os INTEGER;
    v_id_veiculo INTEGER;
    v_id_recepcao INTEGER;
    v_agora TIMESTAMP := now() AT TIME ZONE 'UTC';
BEGIN
    SELECT id_veiculo INTO STRICT v_id_veiculo FROM veiculo WHERE placa = 'FFF2026';

    SELECT c.id_colaborador INTO STRICT v_id_recepcao
    FROM colaborador c JOIN usuario u ON u.id_usuario = c.id_usuario
    WHERE u.login = 'recepcao@oficina.com';

    --OS
    INSERT INTO os (id_veiculo)
    VALUES (v_id_veiculo)
    RETURNING id_os INTO v_id_os;

    --OS COLABORADOR
    INSERT INTO os_colaborador (id_os, id_colaborador)
    VALUES
        (v_id_os, v_id_recepcao);

    --OS STATUS (histórico até Aguardando aprovação)
    INSERT INTO os_status (id_os, id_status, id_colaborador, data_inclusao)
    SELECT v_id_os, st.id_status, historico.id_colaborador, historico.data_inclusao
    FROM (VALUES
        ('Recebida', v_id_recepcao, v_agora - INTERVAL '2 hours')
    ) AS historico (nome, id_colaborador, data_inclusao)
    JOIN status st ON st.nome = historico.nome;
END $$;
