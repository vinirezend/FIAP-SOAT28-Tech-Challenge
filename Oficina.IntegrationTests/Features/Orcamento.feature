# language: pt
Funcionalidade: Envio de orçamento ao cliente
  Para que o cliente autorize a execução dos serviços
  Como colaborador da oficina
  Quero enviar o orçamento da ordem de serviço por e-mail, com botões para aprovar ou recusar

  Contexto:
    Dado que o cliente "Lucas Ferreira" com e-mail "lucas.ferreira@example.com" possui o veículo "Gol" da marca "Volkswagen" de placa "FFF2026"
    E que existe o serviço "Troca de óleo do motor" no valor de R$ 80,00
    E que existe o insumo "Filtro de óleo" no valor de R$ 28,90 com 10 unidades em estoque
    E que estou autenticado como o colaborador "recepcao@oficina.com"
    E que existe uma ordem de serviço aberta para o veículo de placa "FFF2026"
    E que o serviço "Troca de óleo do motor" foi incluído na ordem de serviço com quantidade 1
    E que o insumo "Filtro de óleo" foi incluído na ordem de serviço com quantidade 2

  Cenário: Orçamento é enviado automaticamente quando a OS passa para Aguardando aprovação
    Dado que a ordem de serviço está com status "Em diagnóstico"
    Quando eu alterar o status da ordem de serviço para "Aguardando aprovação"
    Então devo receber o status 200
    E a ordem de serviço deve ter o valor de R$ 137,80
    E o cliente deve receber um e-mail com o assunto "Oficina - Orçamento da sua ordem de serviço"
    E o e-mail deve conter "Olá, Lucas Ferreira!"
    E o e-mail deve conter "Volkswagen Gol (FFF2026)"
    E o e-mail deve conter "Troca de óleo do motor 1 R$ 80,00 R$ 80,00"
    E o e-mail deve conter "Filtro de óleo 2 R$ 28,90 R$ 57,80"
    E o e-mail deve conter "Total do orçamento R$ 137,80"
    E o e-mail deve conter "Aprovar orçamento"
    E o e-mail deve conter "Recusar orçamento"

  Cenário: Cliente aprova o orçamento pelo link do e-mail
    Dado que a ordem de serviço está com status "Aguardando aprovação"
    E que o cliente recebeu o e-mail com o assunto "Oficina - Orçamento da sua ordem de serviço"
    Quando o cliente abrir o link "Aprovar orçamento" do e-mail
    Então devo receber o status 200
    E a página deve exibir "Deseja aprovar o orçamento no valor de R$ 137,80?"
    Quando o cliente confirmar a ação na página aberta pelo link
    Então devo receber o status 200
    E a página deve exibir "Orçamento aprovado"
    E a ordem de serviço deve estar com status "Em execução"
    E a ordem de serviço deve ter a data de início da execução preenchida
    E o histórico da ordem de serviço deve ter os status "Recebida, Em diagnóstico, Aguardando aprovação, Em execução" registrados pelo colaborador autenticado

  Cenário: Cliente recusa o orçamento pelo link do e-mail
    Dado que a ordem de serviço está com status "Aguardando aprovação"
    E que o cliente recebeu o e-mail com o assunto "Oficina - Orçamento da sua ordem de serviço"
    Quando o cliente abrir o link "Recusar orçamento" do e-mail
    E o cliente confirmar a ação na página aberta pelo link
    Então devo receber o status 200
    E a página deve exibir "Orçamento recusado"
    E a ordem de serviço deve estar com status "Finalizada"
    E a ordem de serviço deve ter a data de fim preenchida sem data de início da execução
    E o insumo "Filtro de óleo" deve ter 0 unidades reservadas

  Cenário: Orçamento já respondido não pode ser respondido novamente
    Dado que a ordem de serviço está com status "Aguardando aprovação"
    E que o cliente recebeu o e-mail com o assunto "Oficina - Orçamento da sua ordem de serviço"
    Quando o cliente abrir o link "Aprovar orçamento" do e-mail
    E o cliente confirmar a ação na página aberta pelo link
    E o cliente abrir o link "Recusar orçamento" do e-mail
    Então devo receber o status 200
    E a página deve exibir "Orçamento já respondido"
    E a ordem de serviço deve estar com status "Em execução"

  Cenário: Oficina reenvia o orçamento ao cliente
    Dado que a ordem de serviço está com status "Aguardando aprovação"
    Quando eu reenviar o orçamento da ordem de serviço ao cliente
    Então devo receber o status 200
    E o cliente deve ter recebido 2 e-mails com o assunto "Oficina - Orçamento da sua ordem de serviço"
    E o e-mail deve conter "Total do orçamento R$ 137,80"

  Cenário: Orçamento não é enviado antes do diagnóstico ser concluído
    Quando eu reenviar o orçamento da ordem de serviço ao cliente
    Então devo receber o status 400
    E nenhum e-mail deve ser enviado
