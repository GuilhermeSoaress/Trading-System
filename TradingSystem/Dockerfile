FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TradingSystem/TradingSystem.csproj TradingSystem/
RUN dotnet restore TradingSystem/TradingSystem.csproj

COPY TradingSystem/ TradingSystem/
WORKDIR /src/TradingSystem
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingSystem.dll"]
