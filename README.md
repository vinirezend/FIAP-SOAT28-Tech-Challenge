# FIAP-SOAT28 - Tech Challenge

API de gestão de oficina desenvolvida em **.NET 10**, com **PostgreSQL** como banco de dados.

## Estrutura

| Projeto | Responsabilidade |
|---|---|
| `Oficina.Api` | Web API (controllers, configuração, entrada da aplicação) |
| `Oficina.Application` | Casos de uso / regras de aplicação |
| `Oficina.Domain` | Entidades e regras de domínio |
| `Oficina.Infrastructure` | Acesso a dados (Dapper + Npgsql) |
| `Oficina.Tests` | Testes unitários (xUnit) |
| `Oficina.IntegrationTests` | Testes integrados em BDD (Reqnroll + Testcontainers) — ver [Testes](#testes) |

## Containers

O `docker-compose.yml` sobe dois serviços:

| Serviço | Imagem | Porta (host) | Descrição |
|---|---|---|---|
| `api` | `oficina-api:latest` (build de `Oficina.Api/Dockerfile`) | `8080` | Web API |
| `postgres` | `postgres:17-alpine` | `5432` | Banco de dados (volume `postgres-data`) |

A API só inicia depois que o PostgreSQL estiver saudável (healthcheck com `pg_isready`) e recebe a connection string via variável de ambiente `ConnectionStrings__DefaultConnection`.

### Variáveis de ambiente

Os valores padrão já funcionam sem configuração. Para personalizar, copie o `.env.example` para `.env` (o `.env` não é versionado):

```bash
cp .env.example .env
```

| Variável | Padrão | Descrição |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Ambiente da API |
| `API_PORT` | `8080` | Porta da API no host |
| `POSTGRES_DB` | `oficina` | Nome do banco |
| `POSTGRES_USER` | `oficina` | Usuário do banco |
| `POSTGRES_PASSWORD` | `oficina` | Senha do banco |
| `POSTGRES_PORT` | `5432` | Porta do PostgreSQL no host |
| `DB_INIT_SCRIPTS_PATH` | `./database/init` | Pasta com scripts de inicialização do banco |

### Scripts de inicialização do banco

Para criar tabelas ou inserir dados iniciais, adicione arquivos `.sql` (ou `.sh`) em [`database/init`](database/init/README.md). Eles são executados automaticamente pelo PostgreSQL, em ordem alfabética, **apenas na primeira inicialização** (volume vazio). Use prefixo numérico para controlar a ordem, ex.: `001_create_tables.sql`, `002_seed_data.sql`.

Para reexecutar os scripts, recrie o volume (apaga os dados):

```bash
docker compose down -v
docker compose up -d --build
```

> As senhas dos usuários são gravadas como hash PBKDF2-SHA256 (o seed em `013_insert_inicial.sql` já traz os hashes, com a senha em texto num comentário). Um volume criado antes dessa mudança ainda tem as senhas em texto puro e **o login falha** até recriar o volume com os comandos acima.

### Segredos

Os segredos não ficam no `appsettings.json`. No ambiente `Development` a API usa valores de desenvolvimento do `appsettings.Development.json`; em qualquer outro ambiente ela **não inicia** sem eles:

| Configuração | Variável de ambiente | Regra |
|---|---|---|
| Chave de assinatura do JWT | `TokenConfigurations__Secret` | obrigatória, mínimo 32 caracteres |
| Chave dos links assinados | `LinkAssinadoConfigurations__Secret` | obrigatória, mínimo 32 caracteres |
| Usuário / chave do SMTP | `EmailConfigurations__User` / `EmailConfigurations__Key` | obrigatórias |

Para usar o SMTP real ao rodar localmente, guarde as credenciais no user-secrets (fora do repositório) em vez do `launchSettings.json`:

```bash
dotnet user-secrets set "EmailConfigurations:User" "<usuario>" --project Oficina.Api
dotnet user-secrets set "EmailConfigurations:Key" "<chave>" --project Oficina.Api
```

---

## Opção 1 — Docker Desktop (Windows / macOS)

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução
- No Windows, recomenda-se o backend **WSL 2** (Settings → General → *Use the WSL 2 based engine*)

### Executando

Na raiz do repositório (PowerShell, CMD ou terminal):

```bash
docker compose up -d --build
```

Verifique os containers:

```bash
docker compose ps
```

---

## Opção 2 — WSL (Docker Engine dentro do Linux, sem Docker Desktop)

### Pré-requisitos

- WSL 2 com uma distribuição Linux (ex.: Ubuntu) — [guia de instalação do WSL](https://learn.microsoft.com/pt-br/windows/wsl/install)
- Docker Engine com o plugin Compose instalado na distribuição — [instalação do Docker Engine no Ubuntu](https://docs.docker.com/engine/install/ubuntu/) e [pós-instalação no Linux](https://docs.docker.com/engine/install/linux-postinstall/) (para usar o Docker sem `sudo`)

### Executando

Para melhor performance de build, clone o repositório no sistema de arquivos do Linux (ex.: `~/repos`) em vez de `/mnt/c/...`:

```bash
mkdir -p ~/repos && cd ~/repos
git clone https://github.com/vinirezend/FIAP-SOAT28-Tech-Challenge.git
cd FIAP-SOAT28-Tech-Challenge
docker compose up -d --build
```

As portas publicadas no WSL 2 ficam acessíveis pelo Windows via `localhost`.

---

## Acessando a API

Com os containers em execução (em qualquer uma das opções):

| Recurso | URL |
|---|---|
| API | <http://localhost:8080> |
| Swagger UI (somente em `Development`) | <http://localhost:8080/swagger> |
| Documento OpenAPI (somente em `Development`) | <http://localhost:8080/openapi/v1.json> |
| Health check — liveness | <http://localhost:8080/health/live> |
| Health check — readiness (inclui PostgreSQL) | <http://localhost:8080/health/ready> |

### Health checks

- `/health/live`: indica apenas que a aplicação está no ar; não verifica dependências.
- `/health/ready`: verifica a aplicação e suas dependências (PostgreSQL). Retorna `200` quando saudável e `503` quando alguma dependência falha.

Exemplo de resposta:

```json
{
  "status": "Healthy",
  "totalDuration": 12.4,
  "checks": [
    { "name": "postgres", "status": "Healthy", "duration": 11.9, "description": null, "error": null }
  ]
}
```

Conexão com o PostgreSQL por um cliente externo (DBeaver, pgAdmin etc.):

| Campo | Valor |
|---|---|
| Host | `localhost` |
| Porta | `5432` |
| Banco | `oficina` |
| Usuário / Senha | `oficina` / `oficina` |

## Comandos úteis

```bash
# Logs da API em tempo real
docker compose logs -f api

# Parar os containers (mantém os dados)
docker compose down

# Parar e apagar os dados do banco
docker compose down -v

# Rebuild apenas da API após alterações no código
docker compose up -d --build api
```

## Executando localmente (sem container para a API)

Útil para debugar pelo Visual Studio / Rider / VS Code. Requer o [.NET 10 SDK](https://dotnet.microsoft.com/download).

1. Suba apenas o banco:

   ```bash
   docker compose up -d postgres
   ```

2. Execute a API (usa a connection string de `appsettings.Development.json`, apontando para `localhost:5432`):

   ```bash
   dotnet run --project Oficina.Api
   ```

   A API ficará disponível em <http://localhost:5078> (Swagger em <http://localhost:5078/swagger>).

3. Testes: ver a seção [Testes](#testes).

## Testes

```bash
# Todos os testes (unitários + integrados) — os integrados exigem Docker acessível
dotnet test

# Apenas os unitários (não precisam de Docker)
dotnet test Oficina.Tests

# Apenas os integrados
dotnet test Oficina.IntegrationTests
```

### Testes integrados

Os testes em `Oficina.IntegrationTests` sobem a API em memória (`WebApplicationFactory`) contra um **PostgreSQL real** criado pelo [Testcontainers](https://dotnet.testcontainers.org/), e os cenários são escritos em Gherkin (português) com o [Reqnroll](https://reqnroll.net/) (sucessor do SpecFlow).

Como funciona:

1. No início da execução, um container `postgres:17-alpine` é criado e os scripts de [`database/init`](database/init/README.md) são executados — o schema dos testes é o mesmo do `docker-compose`.
2. Antes de cada cenário, o [Respawn](https://github.com/jbogard/Respawn) apaga os dados de todas as tabelas (o schema é mantido).
3. Ao final, o container é removido. Nada é gravado no banco do `docker-compose`.

Estrutura:

```
Oficina.IntegrationTests/
├── Features/          # cenários .feature em português (ex.: Login.feature)
├── Steps/             # step definitions (Dado / Quando / Então)
├── Hooks/             # sobe banco + API uma vez; limpa o banco antes de cada cenário
├── Infrastructure/    # PostgresDatabase (Testcontainers + Respawn) e OficinaApiFactory
└── Support/           # ApiContext (HttpClient por cenário) e seeders de dados
```

Para criar um novo cenário: adicione um `.feature` em `Features/` (começando com `# language: pt`), implemente os passos em `Steps/` e, se precisar de dados, crie um seeder em `Support/`. Os cenários não rodam em paralelo, pois compartilham o mesmo banco.

#### Pré-requisito: Docker acessível pelo processo de teste

O Testcontainers precisa falar com um daemon Docker **a partir do mesmo ambiente em que o `dotnet test` roda**:

| Onde roda o `dotnet test` | Requisito |
|---|---|
| Windows (Visual Studio, Rider, PowerShell) | **Docker Desktop** instalado e em execução ([Opção 1](#opção-1--docker-desktop-windows--macos)) |
| Dentro do WSL | Docker Engine no WSL ([Opção 2](#opção-2--wsl-docker-engine-dentro-do-linux-sem-docker-desktop)) **e** o .NET 10 SDK instalado no WSL (abaixo) |
| GitHub Actions (`ubuntu-latest`) | Nada — o runner já tem Docker |

Se o Docker só existir dentro do WSL e os testes forem executados pelo Windows, todos falham com `DockerUnavailableException: Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'`.

#### Executando pelo WSL (sem Docker Desktop)

1. Instale o .NET 10 SDK no WSL (apenas para o seu usuário, sem `sudo`):

   ```bash
   curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
   bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
   cat >> ~/.bashrc <<'EOF'
   export DOTNET_ROOT=$HOME/.dotnet
   export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
   EOF
   source ~/.bashrc
   ```

2. Rode os testes a partir de um clone no sistema de arquivos do Linux (ex.: `~/repos/FIAP-SOAT28-Tech-Challenge`):

   ```bash
   dotnet test Oficina.IntegrationTests
   ```

> Evite rodar `dotnet test` no WSL diretamente em `/mnt/c/...` sobre o mesmo checkout usado no Windows: as pastas `bin/` e `obj/` passam a ser compartilhadas entre Linux e Windows e o build de um quebra o do outro (exige `dotnet restore` a cada troca).
