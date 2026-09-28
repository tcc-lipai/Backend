# Etapa de compilação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BancodeDados/Backend.csproj BancodeDados/
RUN dotnet restore BancodeDados/Backend.csproj

COPY BancodeDados/ BancodeDados/
RUN dotnet publish BancodeDados/Backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa de execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["dotnet", "Backend.dll"]
