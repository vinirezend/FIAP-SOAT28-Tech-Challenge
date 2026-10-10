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

- API: <http://localhost:8080>
- Exemplo: <http://localhost:8080/weatherforecast>
- Documento OpenAPI (somente em `Development`): <http://localhost:8080/openapi/v1.json>

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

   A API ficará disponível em <http://localhost:5078>.

3. Testes:

   ```bash
   dotnet test
   ```
