# Scripts de inicialização do banco

Os arquivos desta pasta são montados em `/docker-entrypoint-initdb.d` no container do PostgreSQL e executados automaticamente **somente na primeira inicialização** (quando o volume `postgres-data` ainda está vazio).

- Extensões suportadas: `.sql`, `.sql.gz`, `.sql.xz`, `.sql.zst` e `.sh`
- Ordem de execução: alfabética — use prefixo numérico, ex.:
  - `001_create_tables.sql`
  - `002_seed_data.sql`
- Os scripts rodam no banco definido em `POSTGRES_DB`, com o usuário `POSTGRES_USER`
- Se um script falhar, a inicialização do container é interrompida

Para executar os scripts novamente, é preciso recriar o volume do banco (**apaga todos os dados**):

```bash
docker compose down -v
docker compose up -d --build
```
