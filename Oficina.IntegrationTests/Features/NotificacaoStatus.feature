# language: pt
Funcionalidade: Notificação ao cliente sobre alteração de status
  Para que o cliente saiba em que etapa está o seu veículo
  Como colaborador da oficina
  Quero avançar o status da ordem de serviço e enviar ao cliente o link de acompanhamento

  Contexto:
    Dado que o cliente "Lucas Ferreira" com e-mail "lucas.ferreira@example.com" possui o veículo "Gol" da marca "Volkswagen" de placa "FFF2026"
    E que estou autenticado como o colaborador "recepcao@oficina.com"
    E que existe uma ordem de serviço aberta para o veículo de placa "FFF2026"

  Cenário: Cliente é notificado do novo status da ordem de serviço
    Quando eu alterar o status da ordem de serviço para "Em diagnóstico"
    E eu enviar o acompanhamento da ordem de serviço ao cliente
    Então devo receber o status 200
    E o cliente deve receber um e-mail com o assunto "Oficina - Acompanhe sua ordem de serviço"
    E o e-mail deve conter "Olá, Lucas Ferreira!"
    E o e-mail deve conter "Volkswagen Gol (FFF2026)"
    E o e-mail deve conter "Status atual: Em diagnóstico"
    Quando o cliente abrir o link "Acompanhar ordem de serviço" do e-mail
    Então devo receber o status 200
    E a página deve exibir "Em diagnóstico"

  Cenário: O link de acompanhamento mostra o status atualizado a cada acesso
    Quando eu enviar o acompanhamento da ordem de serviço ao cliente
    Então o cliente deve receber um e-mail com o assunto "Oficina - Acompanhe sua ordem de serviço"
    E o e-mail deve conter "Status atual: Recebida"
    Quando eu alterar o status da ordem de serviço para "Em diagnóstico"
    E o cliente abrir o link "Acompanhar ordem de serviço" do e-mail
    Então devo receber o status 200
    E a página deve exibir "Em diagnóstico"
    E o histórico da ordem de serviço deve ter os status "Recebida, Em diagnóstico" registrados pelo colaborador autenticado

  Cenário: Link de acompanhamento adulterado é recusado
    Quando eu enviar o acompanhamento da ordem de serviço ao cliente
    Então o cliente deve receber um e-mail com o assunto "Oficina - Acompanhe sua ordem de serviço"
    Quando o cliente abrir o link "Acompanhar ordem de serviço" do e-mail com a assinatura adulterada
    Então devo receber o status 403
    E a página deve exibir "Link inválido"

  Cenário: Transição de status fora do fluxo não altera a ordem de serviço
    Quando eu alterar o status da ordem de serviço para "Em execução"
    Então devo receber o status 400
    E a ordem de serviço deve estar com status "Recebida"
    E nenhum e-mail deve ser enviado
