# Scripts de inicialização do banco

Os arquivos desta pasta são montados em `/docker-entrypoint-initdb.d` no container do PostgreSQL e executados automaticamente **somente na primeira inicialização** (quando o volume `postgres-data` ainda está vazio).

- Extensões suportadas: `.sql`, `.sql.gz`, `.sql.xz`, `.sql.zst` e `.sh`
- Ordem de execução: alfabética — use prefixo numérico, ex.:
  - `001_create_tables.sql`
  - `002_seed_data.sql`
- Veja o exemplo em [`001_example.sql`](001_example.sql), que cria a tabela `exemplo` com dados iniciais
- Os scripts rodam no banco definido em `POSTGRES_DB`, com o usuário `POSTGRES_USER`
- Se um script falhar, a inicialização do container é interrompida

Para executar um script em um banco já existente, sem perder dados:

```bash
docker exec oficina-postgres psql -U oficina -d oficina -f /docker-entrypoint-initdb.d/001_example.sql
```

Para executar todos os scripts novamente do zero, é preciso recriar o volume do banco (**apaga todos os dados**):

```bash
docker compose down -v
docker compose up -d --build
```
