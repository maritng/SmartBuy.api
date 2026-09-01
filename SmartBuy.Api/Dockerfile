# Imagen de la API de SmartBuy. Contexto de build: la raíz del repo
# (docker compose build lo maneja). El worker de capturas viaja adentro:
# con el contenedor siempre encendido, los bots corren solos cada día.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore primero (cachea la capa mientras no cambien los csproj/feed).
COPY NuGet.config ./
COPY nuget-local/ nuget-local/
COPY SmartBuy.Api/SmartBuy.Api.csproj SmartBuy.Api/
COPY SmartBuy.Core/SmartBuy.Core.csproj SmartBuy.Core/
COPY SmartBuy.Data/SmartBuy.Data.csproj SmartBuy.Data/
RUN dotnet restore SmartBuy.Api/SmartBuy.Api.csproj

COPY . .
RUN dotnet publish SmartBuy.Api/SmartBuy.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartBuy.Api.dll"]
