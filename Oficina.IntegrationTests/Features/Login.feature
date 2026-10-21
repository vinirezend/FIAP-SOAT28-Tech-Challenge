# language: pt
Funcionalidade: Login
  Para acessar os recursos protegidos da API
  Como usuário da oficina
  Quero me autenticar com login e senha e receber um token JWT

  Contexto:
    Dado que existe o usuário "admin" com senha "123456"

  Cenário: Login com credenciais válidas
    Quando eu fizer login com usuário "admin" e senha "123456"
    Então devo receber o status 200
    E a resposta deve conter um token JWT válido para o usuário "admin"

  Esquema do Cenário: Login com credenciais inválidas
    Quando eu fizer login com usuário "<login>" e senha "<senha>"
    Então devo receber o status 401

    Exemplos:
      | descricao           | login   | senha  |
      | senha incorreta     | admin   | errada |
      | usuário inexistente | naoexis | 123456 |

  Cenário: Login sem corpo na requisição
    Quando eu fizer login sem enviar o corpo da requisição
    Então devo receber o status 400
