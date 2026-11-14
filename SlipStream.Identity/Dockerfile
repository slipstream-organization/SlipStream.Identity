# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY global.json ./
COPY SlipStream.Identity/SlipStream.Identity.csproj                         SlipStream.Identity/
COPY SlipStream.Identity.Core/SlipStream.Identity.Core.csproj               SlipStream.Identity.Core/
COPY SlipStream.Identity.Domain/SlipStream.Identity.Domain.csproj           SlipStream.Identity.Domain/
COPY SlipStream.Identity.Infrastructure/SlipStream.Identity.Infrastructure.csproj SlipStream.Identity.Infrastructure/

ARG GH_USER=Kitukl

RUN --mount=type=secret,id=gh_token \
    echo "TEMP-DEBUG user=$GH_USER token_chars=$(wc -c < /run/secrets/gh_token)" \
    && dotnet nuget add source "https://nuget.pkg.github.com/slipstream-organization/index.json" \
        --name github \
        --username "$GH_USER" \
        --password "$(cat /run/secrets/gh_token)" \
        --store-password-in-clear-text \
    && dotnet restore SlipStream.Identity/SlipStream.Identity.csproj \
    && dotnet nuget remove source github

COPY . .
RUN dotnet publish SlipStream.Identity/SlipStream.Identity.csproj \
        -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID

HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
    CMD wget -q --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "SlipStream.Identity.dll"]
