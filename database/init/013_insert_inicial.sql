--USUARIO
INSERT INTO usuario
    (login, senha, ativo)
VALUES
    ('gerente@oficina.com', 'gerente123!', TRUE),
    ('recepcao@oficina.com', 'recepcao123!', TRUE),
    ('oficina@oficina.com', 'oficina123!', TRUE),
    ('almoxarifado@oficina.com', 'almoxarifado123!', TRUE);

--CLIENTE
INSERT INTO cliente 
    (nome, numero_documento, tipo_documento, email, telefone)
VALUES
    ('Lucas Ferreira', '32887790056', 1, 'lucas.ferreira@example.com', '(19) 98888-8888'),
    ('Mariana Oliveira', '11319526000155', 2, 'contato@marianaautos.example.com', '(19) 3333-3333'),
    ('Rafael Costa', '95475931020', 1, 'rafael.costa@example.com', '(19) 99999-9999');

--VEICULO
INSERT INTO veiculo 
    (id_cliente, placa, modelo, marca, ano, id_tipo_veiculo)
VALUES 
    (1, 'FFF2026', 'Gol', 'Volkswagen', 2019, 1),
    (1, 'VVV1V23', 'CG 160 Fan', 'Honda', 2023, 2),
    (2, 'GGG2026', 'Onix', 'Chevrolet', 2020, 1),
    (2, 'PPP1P23', 'Accelo 1016', 'Mercedes-Benz', 2022, 3);

--INSUMO
INSERT INTO insumo 
    (nome, descricao, ativo, quantidade_estoque, quantidade_reservada, valor_unitario)
VALUES
    ('Óleo de motor 5W30', 'Óleo lubrificante sintético para motores compatíveis', TRUE, 40, 0, 42.90),
    ('Óleo de motor 10W40', 'Óleo lubrificante semissintético para motores compatíveis', TRUE, 30, 0, 32.50),
    ('Filtro de óleo', 'Filtro de óleo para motores automotivos', TRUE, 25, 0, 28.90),
    ('Filtro de ar', 'Elemento filtrante para admissão de ar do motor', TRUE, 20, 0, 45.00),
    ('Filtro de combustível', 'Filtro para retenção de impurezas do combustível', TRUE, 15, 0, 38.50),
    ('Filtro de cabine', 'Filtro de ar condicionado e ventilação interna', TRUE, 18, 0, 35.90),
    ('Pastilha de freio dianteira', 'Jogo de pastilhas para freio a disco dianteiro', TRUE, 12, 0, 145.00),
    ('Pastilha de freio traseira', 'Jogo de pastilhas para freio traseiro', TRUE, 10, 0, 125.00),
    ('Disco de freio', 'Disco de freio para veículos compatíveis', TRUE, 8, 0, 189.90),
    ('Fluido de freio DOT 4', 'Fluido hidráulico para sistema de freios', TRUE, 20, 0, 29.90),
    ('Aditivo para radiador', 'Aditivo para sistema de arrefecimento', TRUE, 15, 0, 24.90),
    ('Água desmineralizada', 'Água desmineralizada para uso automotivo', TRUE, 25, 0, 8.90),
    ('Vela de ignição', 'Vela de ignição para motores compatíveis', TRUE, 32, 0, 22.50),
    ('Correia dentada', 'Correia de sincronismo do motor', TRUE, 6, 0, 115.00),
    ('Correia de acessórios', 'Correia para acionamento de acessórios do motor', TRUE, 10, 0, 65.00),
    ('Lâmpada automotiva H7', 'Lâmpada para farol de veículos compatíveis', TRUE, 20, 0, 19.90),
    ('Fusível automotivo', 'Fusível para circuitos elétricos automotivos', TRUE, 100, 0, 2.50),
    ('Bateria automotiva 60Ah', 'Bateria de partida de 60 amperes-hora', TRUE, 5, 0, 489.90),
    ('Graxa lubrificante', 'Graxa para lubrificação de componentes mecânicos', TRUE, 10, 0, 32.90),
    ('Jogo de juntas do motor', 'Conjunto de juntas para motor compatível', TRUE, 4, 0, 220.00);

--SERVICO
INSERT INTO servico 
    (nome, descricao, ativo, valor_estipulado)
VALUES
    ('Troca de óleo do motor', 'Drenagem do óleo usado e substituição do lubrificante', TRUE, 80.00),
    ('Troca de filtro de óleo', 'Remoção e instalação do filtro de óleo do motor', TRUE, 35.00),
    ('Troca de filtro de ar', 'Substituição do elemento filtrante de admissão', TRUE, 25.00),
    ('Troca de filtro de combustível', 'Substituição do filtro de combustível', TRUE, 50.00),
    ('Troca de filtro de cabine', 'Substituição do filtro do ar condicionado', TRUE, 35.00),
    ('Troca de pastilhas de freio', 'Substituição das pastilhas de freio de um eixo', TRUE, 140.00),
    ('Troca de discos de freio', 'Substituição dos discos de freio de um eixo', TRUE, 180.00),
    ('Sangria do sistema de freios', 'Remoção de ar do circuito hidráulico de freios', TRUE, 100.00),
    ('Troca de fluido de freio', 'Substituição do fluido do sistema de freios', TRUE, 120.00),
    ('Revisão do sistema de arrefecimento', 'Inspeção de mangueiras, reservatório e circulação', TRUE, 150.00),
    ('Troca de aditivo do radiador', 'Drenagem e reposição do fluido de arrefecimento', TRUE, 100.00),
    ('Substituição de velas de ignição', 'Remoção e instalação das velas do motor', TRUE, 90.00),
    ('Troca de correia dentada', 'Substituição da correia de sincronismo do motor', TRUE, 450.00),
    ('Troca de correia de acessórios', 'Substituição da correia de acionamento de acessórios', TRUE, 120.00),
    ('Diagnóstico eletrônico', 'Leitura de códigos de falha com scanner automotivo', TRUE, 120.00),
    ('Alinhamento de direção', 'Ajuste dos ângulos de alinhamento das rodas', TRUE, 100.00),
    ('Balanceamento de rodas', 'Balanceamento das rodas para reduzir vibrações', TRUE, 30.00),
    ('Revisão da suspensão', 'Inspeção de amortecedores, buchas, pivôs e terminais', TRUE, 120.00),
    ('Substituição de bateria', 'Remoção e instalação da bateria automotiva', TRUE, 50.00),
    ('Revisão preventiva completa', 'Inspeção geral dos principais sistemas do veículo', TRUE, 300.00);

--COLABORADOR
INSERT INTO colaborador
    (nome, cpf, email, ativo, id_departamento, id_usuario)
VALUES
    ('Gerente 1', '02994706601', 'gerente@oficina.com', true, 1, 1),
    ('Recepcao 1', '24275977831', 'Recepcao@oficina.com', true, 3, 2),
    ('Oficina 1', '50118487442', 'Oficina@oficina.com', true, 4, 3),
    ('Almoxarifado 1', '10367185571', 'Almoxarifado@oficina.com', true, 5, 4);

--OS
INSERT INTO os
    (id_veiculo)
VALUES
    (1);

--STATUS
INSERT INTO status
    (nome)
VALUES
    ('Recebida'),
    ('Em diagnóstico'),
    ('Aguardando aprovação'),
    ('Em execução'),
    ('Finalizada'),
    ('Entregue');

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