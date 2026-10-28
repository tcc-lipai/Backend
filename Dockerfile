# ---------------------------------------------------------------------------
# LipAI Back-end (ASP.NET Core / .NET 10)
#
# Build:   docker build -t lipai-backend .
# Compose: docker compose up --build        (sobe a API + SQL Server)
#
# Nenhum segredo entra na imagem: Jwt, banco, admin etc. vêm de variáveis de
# ambiente (ver docker-compose.yml e .env.example).
# ---------------------------------------------------------------------------

# Etapa de compilação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# restaura primeiro só com o .csproj: o cache das dependências só invalida se ele mudar
COPY BancodeDados/Backend.csproj BancodeDados/
RUN dotnet restore BancodeDados/Backend.csproj

COPY BancodeDados/ BancodeDados/
RUN dotnet publish BancodeDados/Backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false

# Etapa de execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

COPY --from=build /app/publish .

EXPOSE 10000

# não roda como root (a imagem já traz o usuário "app")
USER $APP_UID

ENTRYPOINT ["dotnet", "Backend.dll"]
