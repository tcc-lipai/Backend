# LipAI — Back-end

API REST do LipAI (ASP.NET Core / .NET 10, EF Core + SQL Server, autenticação JWT).
Conversa com o serviço de IA (`LipAI_LLM`) para avaliar a pronúncia.

## Rodar com Docker (API + SQL Server)

Pré-requisito: [Docker Desktop](https://www.docker.com/products/docker-desktop/) rodando.

```bash
# 1) crie o arquivo de variáveis a partir do modelo e preencha
cp .env.example .env        # (PowerShell: Copy-Item .env.example .env)

# 2) suba tudo (a primeira vez baixa as imagens e compila a API)
docker compose up --build -d

# 3) acompanhe os logs / confirme que subiu
docker compose logs -f api      # procure por "Migrations aplicadas." e "Now listening"
```

A API fica em **http://localhost:5251** (mude com `API_PORT` no `.env`).
No Front, aponte para ela: `VITE_API_BASE_URL=http://localhost:5251/api`.

O que o `.env` precisa ter (veja os comentários no `.env.example`):

| Variável | Para quê |
|---|---|
| `SA_PASSWORD` | senha do SQL Server do container (8+ caracteres, maiúscula, minúscula, número e símbolo) |
| `JWT_KEY` | chave que assina o login, 32+ caracteres aleatórios |
| `ADMIN_EMAIL` / `ADMIN_SENHA_HASH` | conta de administrador (hash BCrypt, entre aspas simples) |

Como funciona:

- **Banco**: o container `lipai-db` guarda os dados no volume `lipai-dados` (sobrevive a
  `docker compose down`; só `docker compose down -v` apaga). A API aplica as *migrations*
  sozinha ao iniciar (`Database__MigrarAoIniciar=true`) e espera o SQL Server ficar pronto.
- **Dados de exemplo**: com `POPULAR_DADOS_EXEMPLO=true` (padrão) entram conquistas, dicionário,
  produtos e um pouco de conteúdo — idempotente. O conteúdo completo (as 26 unidades) está no
  seu banco local; veja "Levar o banco local para o Docker" abaixo.
- **IA**: o `LipAI_LLM` continua rodando na sua máquina (`uvicorn ... --port 8001`); a API a
  acessa por `http://host.docker.internal:8001` (`IA_BASE_URL`).
- **Segredos**: nada sensível entra na imagem (`.dockerignore` exclui `appsettings*.json` e `.env`).
  Tudo vem das variáveis do `.env`.
- **Swagger**: `SWAGGER=true` no `.env` liga a documentação em `http://localhost:5251/`.
- **CORS**: `CORS_ORIGEM` / `CORS_ORIGEM_2` (padrão `5173` e `5174`) são as origens do Front liberadas.

Comandos úteis:

```bash
docker compose ps                 # estado dos containers
docker compose restart api        # reinicia só a API
docker compose down               # para tudo (mantém o banco)
docker compose down -v            # para tudo e APAGA o banco
docker compose up --build -d      # recompila depois de mudar o código
```

Conectar no banco do container (SSMS / Azure Data Studio): servidor `localhost,14330`,
usuário `sa`, a senha do `SA_PASSWORD`.

### Levar o banco local para o Docker

Para o container ficar com o mesmo conteúdo do seu SQL Express local:

```powershell
# 1) backup do banco local (a pasta precisa existir e o SQL Express precisa poder escrever nela)
sqlcmd -S "localhost\SQLEXPRESS" -E -C -Q "BACKUP DATABASE LipaiDB TO DISK='C:\temp\LipaiDB.bak' WITH INIT"

# 2) pare a API pra ela não recriar o banco no meio do processo
docker compose stop api

# 3) copie o backup para dentro do container e restaure
docker cp C:\temp\LipaiDB.bak lipai-db:/var/opt/mssql/data/LipaiDB.bak
docker exec lipai-db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "SUA_SA_PASSWORD" -Q "RESTORE DATABASE LipaiDB FROM DISK='/var/opt/mssql/data/LipaiDB.bak' WITH REPLACE, MOVE 'LipaiDB' TO '/var/opt/mssql/data/LipaiDB.mdf', MOVE 'LipaiDB_log' TO '/var/opt/mssql/data/LipaiDB_log.ldf'"

# 4) suba a API de novo
docker compose start api
```

> O backup contém dados reais de usuários: não suba o `.bak` para o Git (já está no `.gitignore`/`.dockerignore`).

### Só a imagem (sem compose)

```bash
docker build -t lipai-backend .
docker run -p 5251:10000 \
  -e ConnectionStrings__DefaultConnection="Server=SEU_SQL;Database=LipaiDB;User Id=sa;Password=...;TrustServerCertificate=True;" \
  -e Jwt__Key="..." -e Jwt__Issuer=LipAI -e Jwt__Audience=LipAI \
  -e Admin__Email="..." -e Admin__SenhaHash="..." \
  -e UseHttpsRedirection=false -e Database__MigrarAoIniciar=true \
  lipai-backend
```

## Rodar sem Docker (desenvolvimento)

```bash
cd BancodeDados
dotnet run --launch-profile https   # https://localhost:7268
```

Precisa de um `appsettings.json` local (não versionado — use o `appsettings.Example.json` como modelo)
com a conexão do SQL Server, `Jwt:Key`, `Admin:Email` e `Admin:SenhaHash`.
