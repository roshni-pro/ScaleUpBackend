#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY ApiGateways/ScaleUP.ApiGateways.Aggregator/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY ApiGateways/ScaleUP.ApiGateways.Aggregator/https /app/https/

#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
USER app
WORKDIR /app
EXPOSE 5004
EXPOSE 7004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ApiGateways/ScaleUP.ApiGateways.Aggregator/https /src/https/
COPY ["ApiGateways/ScaleUP.ApiGateways.Aggregator/ScaleUP.ApiGateways.Aggregator.csproj", "ApiGateways/ScaleUP.ApiGateways.Aggregator/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
RUN dotnet restore "./ApiGateways/ScaleUP.ApiGateways.Aggregator/./ScaleUP.ApiGateways.Aggregator.csproj"
COPY . .
WORKDIR "/src/ApiGateways/ScaleUP.ApiGateways.Aggregator"
RUN dotnet build "./ScaleUP.ApiGateways.Aggregator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.ApiGateways.Aggregator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.ApiGateways.Aggregator.dll"]