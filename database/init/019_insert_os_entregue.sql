-- OS completa para testes locais do fluxo de orçamento:
-- veículo FFF2026 (Gol do cliente Lucas Ferreira), com serviços e insumos,
-- histórico Recebida -> Em diagnóstico -> Aguardando aprovação -> Em execução -> finalizada -> Entregue, valor calculado
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
    SELECT id_veiculo INTO STRICT v_id_veiculo FROM veiculo WHERE placa = 'FFF2026';

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
        ('Balanceamento de rodas', 1)
    ) AS item (nome, quantidade)
    JOIN servico s ON s.nome = item.nome;

    --VALOR DA OS (congelado ao entrar em Aguardando aprovação)
    UPDATE os
    SET valor = (SELECT COALESCE(SUM(quantidade * valor_cobrado), 0) FROM os_servico WHERE id_os = v_id_os)
    WHERE id_os = v_id_os;

    --DATA DE INICIO DA OS (Congelado ao entrar em execução)
    UPDATE os
    SET data_inicio = v_agora - INTERVAL '3 hours',
        data_fim = v_agora - INTERVAL '50 minutes'
    WHERE id_os = v_id_os;

    --OS STATUS (histórico até Aguardando aprovação)
    INSERT INTO os_status (id_os, id_status, id_colaborador, data_inclusao)
    SELECT v_id_os, st.id_status, historico.id_colaborador, historico.data_inclusao
    FROM (VALUES
        ('Recebida', v_id_recepcao, v_agora - INTERVAL '1 day'),
        ('Em diagnóstico', v_id_oficina, v_agora - INTERVAL '22 hours'),
        ('Aguardando aprovação', v_id_oficina, v_agora - INTERVAL '20 hours'),
        ('Em execução', v_id_oficina, v_agora - INTERVAL '3 hours'),
        ('Finalizada', v_id_oficina, v_agora - INTERVAL '50 minutes'),
        ('Entregue', v_id_recepcao, v_agora - INTERVAL '20 minutes')
    ) AS historico (nome, id_colaborador, data_inclusao)
    JOIN status st ON st.nome = historico.nome;
END $$;
