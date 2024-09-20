#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY Services/Identity/ScaleUP.Services.IdentityAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/Identity/ScaleUP.Services.IdentityAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
USER app
WORKDIR /app
EXPOSE 5008
EXPOSE 7008

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/Identity/ScaleUP.Services.IdentityAPI/https /src/https/
COPY ["Services/Identity/ScaleUP.Services.IdentityAPI/ScaleUP.Services.IdentityAPI.csproj", "Services/Identity/ScaleUP.Services.IdentityAPI/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
COPY ["Services/Identity/ScaleUP.Services.IdentityDTO/ScaleUP.Services.IdentityDTO.csproj", "Services/Identity/ScaleUP.Services.IdentityDTO/"]
COPY ["Services/Identity/ScaleUP.Services.IdentityModels/ScaleUP.Services.IdentityModels.csproj", "Services/Identity/ScaleUP.Services.IdentityModels/"]
RUN dotnet restore "./Services/Identity/ScaleUP.Services.IdentityAPI/./ScaleUP.Services.IdentityAPI.csproj"
COPY . .
WORKDIR "/src/Services/Identity/ScaleUP.Services.IdentityAPI"
RUN dotnet build "./ScaleUP.Services.IdentityAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.IdentityAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.IdentityAPI.dll"]