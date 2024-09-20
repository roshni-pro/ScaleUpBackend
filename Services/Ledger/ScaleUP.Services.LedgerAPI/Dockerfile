#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
COPY Services/Ledger/ScaleUP.Services.LedgerAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/Ledger/ScaleUP.Services.LedgerAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
USER app
WORKDIR /app
EXPOSE 5012
EXPOSE 7012

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/Ledger/ScaleUP.Services.LedgerAPI/https /src/https/
COPY ["Services/Ledger/ScaleUP.Services.LedgerAPI/ScaleUP.Services.LedgerAPI.csproj", "Services/Ledger/ScaleUP.Services.LedgerAPI/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
COPY ["HealthCheck/ScaleUp.HealthCheck.HealthCheckService/ScaleUp.HealthCheck.HealthCheckService.csproj", "HealthCheck/ScaleUp.HealthCheck.HealthCheckService/"]
RUN dotnet restore "./Services/Ledger/ScaleUP.Services.LedgerAPI/ScaleUP.Services.LedgerAPI.csproj"
COPY . .
WORKDIR "/src/Services/Ledger/ScaleUP.Services.LedgerAPI"
RUN dotnet build "./ScaleUP.Services.LedgerAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.LedgerAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.LedgerAPI.dll"]