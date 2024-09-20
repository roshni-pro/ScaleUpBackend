#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY Services/Company/ScaleUP.Services.CompanyAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/Company/ScaleUP.Services.CompanyAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
USER app
WORKDIR /app
EXPOSE 5002
EXPOSE 7002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/Company/ScaleUP.Services.CompanyAPI/https /src/https/
COPY ["Services/Company/ScaleUP.Services.CompanyAPI/ScaleUP.Services.CompanyAPI.csproj", "Services/Company/ScaleUP.Services.CompanyAPI/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
COPY ["Services/Company/ScaleUP.Services.CompanyDTO/ScaleUP.Services.CompanyDTO.csproj", "Services/Company/ScaleUP.Services.CompanyDTO/"]
COPY ["Services/Company/ScaleUP.Services.CompanyModels/ScaleUP.Services.CompanyModels.csproj", "Services/Company/ScaleUP.Services.CompanyModels/"]
RUN dotnet restore "./Services/Company/ScaleUP.Services.CompanyAPI/./ScaleUP.Services.CompanyAPI.csproj"
COPY . .
WORKDIR "/src/Services/Company/ScaleUP.Services.CompanyAPI"
RUN dotnet build "./ScaleUP.Services.CompanyAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.CompanyAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.CompanyAPI.dll"]