-- OS completa para testes locais do fluxo de orçamento:
-- veículo PPP1P23 (Accelo do cliente Rafael Costa), com serviços e insumos,
-- histórico Recebida -> Em diagnóstico -> Aguardando aprovação -> em execução, valor calculado
-- e insumos reservados no estoque.
--
-- O e-mail do orçamento não é enviado pelo script. Para enviá-lo use
-- POST /api/orcamento/{idOs}/envio (o id da OS criada é exibido no log do script).
DO $$
DECLARE
    v_id_os INTEGER;
    v_id_veiculo INTEGER;
    v_id_recepcao INTEGER;
    v_id_oficina INTEGER;
    v_agora TIMESTAMP := now() AT TIME ZONE 'UTC';
BEGIN
    SELECT id_veiculo INTO STRICT v_id_veiculo FROM veiculo WHERE placa = 'PPP1P23';

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
        ('Substituição de velas de ignição', 1)
    ) AS item (nome, quantidade)
    JOIN servico s ON s.nome = item.nome;

    --OS INSUMO (valor cobrado = valor unitário do catálogo)
    INSERT INTO os_insumo (id_os, id_insumo, quantidade, valor_cobrado)
    SELECT v_id_os, i.id_insumo, item.quantidade, i.valor_unitario
    FROM (VALUES
        ('Vela de ignição', 2)
    ) AS item (nome, quantidade)
    JOIN insumo i ON i.nome = item.nome;

    --RESERVA DOS INSUMOS NO ESTOQUE
    UPDATE insumo i
    SET quantidade_reservada = i.quantidade_reservada + oi.quantidade
    FROM os_insumo oi
    WHERE oi.id_insumo = i.id_insumo AND oi.id_os = v_id_os;

    --VALOR DA OS (congelado ao entrar em Aguardando aprovação)
    UPDATE os
    SET valor = (SELECT COALESCE(SUM(quantidade * valor_cobrado), 0) FROM os_servico WHERE id_os = v_id_os)
              + (SELECT COALESCE(SUM(quantidade * valor_cobrado), 0) FROM os_insumo WHERE id_os = v_id_os)
    WHERE id_os = v_id_os;

    --DATA DE INICIO DA OS (Congelado ao entrar em execução)
    UPDATE os
    SET data_inicio = v_agora - INTERVAL '40 minutes'
    WHERE id_os = v_id_os;

    --OS STATUS (histórico até Aguardando aprovação)
    INSERT INTO os_status (id_os, id_status, id_colaborador, data_inclusao)
    SELECT v_id_os, st.id_status, historico.id_colaborador, historico.data_inclusao
    FROM (VALUES
        ('Recebida', v_id_recepcao, v_agora - INTERVAL '5 hours'),
        ('Em diagnóstico', v_id_oficina, v_agora - INTERVAL '4 hour'),
        ('Aguardando aprovação', v_id_oficina, v_agora - INTERVAL '3 hours'),
        ('Em execução', v_id_oficina, v_agora - INTERVAL '40 minutes')
    ) AS historico (nome, id_colaborador, data_inclusao)
    JOIN status st ON st.nome = historico.nome;
END $$;
