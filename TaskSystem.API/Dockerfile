FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskSystem.API/TaskSystem.API.csproj TaskSystem.API/
COPY TaskSystem.Application/TaskSystem.Application.csproj TaskSystem.Application/
COPY TaskSystem.Domain/TaskSystem.Domain.csproj TaskSystem.Domain/
COPY TaskSystem.Infrastructure/TaskSystem.Infrastructure.csproj TaskSystem.Infrastructure/

RUN dotnet restore TaskSystem.API/TaskSystem.API.csproj

COPY . .

RUN dotnet publish TaskSystem.API/TaskSystem.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /home/app/.aspnet

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "TaskSystem.API.dll"]
