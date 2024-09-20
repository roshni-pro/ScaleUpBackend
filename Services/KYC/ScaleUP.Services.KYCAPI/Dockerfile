#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY Services/KYC/ScaleUP.Services.KYCAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/KYC/ScaleUP.Services.KYCAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
USER app
WORKDIR /app
EXPOSE 5006
EXPOSE 7006

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/KYC/ScaleUP.Services.KYCAPI/https /src/https/
COPY ["Services/KYC/ScaleUP.Services.KYCAPI/ScaleUP.Services.KYCAPI.csproj", "Services/KYC/ScaleUP.Services.KYCAPI/"]
COPY ["BuildingBlocks/EventBus/ScaleUp.BuildingBlocks.EventBus/ScaleUP.BuildingBlocks.EventBus.csproj", "BuildingBlocks/EventBus/ScaleUp.BuildingBlocks.EventBus/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
COPY ["Services/KYC/ScaleUP.Services.KYCDTO/ScaleUP.Services.KYCDTO.csproj", "Services/KYC/ScaleUP.Services.KYCDTO/"]
COPY ["Services/KYC/ScaleUP.Services.KYCModels/ScaleUP.Services.KYCModels.csproj", "Services/KYC/ScaleUP.Services.KYCModels/"]
RUN dotnet restore "./Services/KYC/ScaleUP.Services.KYCAPI/./ScaleUP.Services.KYCAPI.csproj"
COPY . .
WORKDIR "/src/Services/KYC/ScaleUP.Services.KYCAPI"
RUN dotnet build "./ScaleUP.Services.KYCAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.KYCAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.KYCAPI.dll"]