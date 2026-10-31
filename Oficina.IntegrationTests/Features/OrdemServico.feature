# language: pt
Funcionalidade: Abertura de ordem de serviço
  Para registrar a entrada de um veículo na oficina
  Como colaborador da recepção
  Quero abrir uma ordem de serviço vinculada ao veículo do cliente

  Contexto:
    Dado que o cliente "Lucas Ferreira" com e-mail "lucas.ferreira@example.com" possui o veículo "Gol" da marca "Volkswagen" de placa "FFF2026"

  Cenário: Colaborador abre uma ordem de serviço
    Dado que estou autenticado como o colaborador "recepcao@oficina.com"
    Quando eu abrir uma ordem de serviço para o veículo de placa "FFF2026"
    Então a ordem de serviço deve ser criada para o veículo de placa "FFF2026" com status "Recebida"
    E o histórico da ordem de serviço deve ter os status "Recebida" registrados pelo colaborador autenticado

  Cenário: Itens incluídos na ordem de serviço reservam o estoque
    Dado que estou autenticado como o colaborador "recepcao@oficina.com"
    E que existe o insumo "Filtro de óleo" no valor de R$ 28,90 com 10 unidades em estoque
    E que existe uma ordem de serviço aberta para o veículo de placa "FFF2026"
    Quando eu incluir o insumo "Filtro de óleo" na ordem de serviço com quantidade 2
    Então o insumo "Filtro de óleo" deve ter 2 unidades reservadas

  Cenário: Abertura de ordem de serviço exige autenticação
    Quando eu tentar abrir uma ordem de serviço sem estar autenticado
    Então devo receber o status 401
