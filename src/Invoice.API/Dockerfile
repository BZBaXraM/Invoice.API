# syntax=docker/dockerfile:1
# Build context is the repository root (see docker-compose.yml), because this
# project references sibling projects (Invoice.Domain/Application/Infrastructure).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Invoice.Domain/Invoice.Domain.csproj src/Invoice.Domain/
COPY src/Invoice.Application/Invoice.Application.csproj src/Invoice.Application/
COPY src/Invoice.Infrastructure/Invoice.Infrastructure.csproj src/Invoice.Infrastructure/
COPY src/Invoice.API/Invoice.API.csproj src/Invoice.API/
RUN dotnet restore src/Invoice.API/Invoice.API.csproj

COPY src/ src/
RUN dotnet publish src/Invoice.API/Invoice.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app


# pg_dump (pinned to the PGDG major matching the compose postgres:17 service)
# powers the admin database-backup endpoint. The distro's own postgres-client
# is older than the server and refuses to dump it, hence the PGDG repo.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 ca-certificates curl gnupg \
    && curl -fsSL https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor -o /usr/share/keyrings/pgdg.gpg \
    && echo "deb [signed-by=/usr/share/keyrings/pgdg.gpg] http://apt.postgresql.org/pub/repos/apt $(. /etc/os-release && echo "$VERSION_CODENAME")-pgdg main" > /etc/apt/sources.list.d/pgdg.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client-17 \
    && apt-get purge -y curl gnupg \
    && apt-get autoremove -y \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# docker-compose overrides ASPNETCORE_URLS to also bind 8081 over HTTPS,
# using a dev cert mounted from the host at /https (see docker-compose.yml).
#EXPOSE 8081

ENTRYPOINT ["dotnet", "Invoice.API.dll"]
