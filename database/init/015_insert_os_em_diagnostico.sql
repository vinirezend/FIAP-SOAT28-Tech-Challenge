-- OS completa para testes locais do fluxo de orçamento:
-- veículo VVV1V23 (CG da cliente Lucas Ferreira), com serviços e insumos,
-- histórico Recebida -> Em diagnóstico
-- e insumos reservados no estoque.
DO $$
DECLARE
    v_id_os INTEGER;
    v_id_veiculo INTEGER;
    v_id_recepcao INTEGER;
    v_id_oficina INTEGER;
    v_agora TIMESTAMP := now() AT TIME ZONE 'UTC';
BEGIN
    SELECT id_veiculo INTO STRICT v_id_veiculo FROM veiculo WHERE placa = 'VVV1V23';

    SELECT c.id_colaborador INTO STRICT v_id_recepcao
    FROM colaborador c JOIN usuario u ON u.id_usuario = c.id_usuario
    WHERE u.login = 'recepcao@oficina.com';

    SELECT c.id_colaborador INTO STRICT v_id_oficina
    FROM colaborador c JOIN usuario u ON u.id_usuario = c.id_usuario
    WHERE u.login = 'oficina@oficina.com';

    --OS
    INSERT INTO os (id_veiculo)
    VALUES (v_id_veiculo)
    RETURNING id_os INTO v_id_os;

    --OS COLABORADOR
    INSERT INTO os_colaborador (id_os, id_colaborador)
    VALUES
        (v_id_os, v_id_recepcao),
        (v_id_os, v_id_oficina);

    --OS SERVICO (valor cobrado = valor estipulado do catálogo)
    INSERT INTO os_servico (id_os, id_servico, quantidade, valor_cobrado)
    SELECT v_id_os, s.id_servico, item.quantidade, s.valor_estipulado
    FROM (VALUES
        ('Troca de pastilhas de freio', 1),
        ('Sangria do sistema de freios', 1)
    ) AS item (nome, quantidade)
    JOIN servico s ON s.nome = item.nome;

    --OS INSUMO (valor cobrado = valor unitário do catálogo)
    INSERT INTO os_insumo (id_os, id_insumo, quantidade, valor_cobrado)
    SELECT v_id_os, i.id_insumo, item.quantidade, i.valor_unitario
    FROM (VALUES
        ('Pastilha de freio dianteira', 1),
        ('Fluido de freio DOT 4', 2)
    ) AS item (nome, quantidade)
    JOIN insumo i ON i.nome = item.nome;

    --RESERVA DOS INSUMOS NO ESTOQUE
    UPDATE insumo i
    SET quantidade_reservada = i.quantidade_reservada + oi.quantidade
    FROM os_insumo oi
    WHERE oi.id_insumo = i.id_insumo AND oi.id_os = v_id_os;

    --OS STATUS (histórico até Aguardando aprovação)
    INSERT INTO os_status (id_os, id_status, id_colaborador, data_inclusao)
    SELECT v_id_os, st.id_status, historico.id_colaborador, historico.data_inclusao
    FROM (VALUES
        ('Recebida', v_id_recepcao, v_agora - INTERVAL '2 hours'),
        ('Em diagnóstico', v_id_oficina, v_agora - INTERVAL '1 hour')
    ) AS historico (nome, id_colaborador, data_inclusao)
    JOIN status st ON st.nome = historico.nome;
END $$;
