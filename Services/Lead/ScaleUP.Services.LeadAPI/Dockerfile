#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base


USER root
COPY Services/Lead/ScaleUP.Services.LeadAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/Lead/ScaleUP.Services.LeadAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
RUN chown app /app
RUN apt-get update && apt-get install -y libgdiplus
USER app
WORKDIR /app
EXPOSE 5005
EXPOSE 7005

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/Lead/ScaleUP.Services.LeadAPI/https /src/https/
COPY ["Services/Lead/ScaleUP.Services.LeadAPI/ScaleUP.Services.LeadAPI.csproj", "Services/Lead/ScaleUP.Services.LeadAPI/"]
COPY ["BuildingBlocks/EventBus/ScaleUp.BuildingBlocks.EventBus/ScaleUP.BuildingBlocks.EventBus.csproj", "BuildingBlocks/EventBus/ScaleUp.BuildingBlocks.EventBus/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
COPY ["Services/Lead/ScaleUP.Services.LeadDTO/ScaleUP.Services.LeadDTO.csproj", "Services/Lead/ScaleUP.Services.LeadDTO/"]
COPY ["Services/Lead/ScaleUP.Services.LeadModels/ScaleUP.Services.LeadModels.csproj", "Services/Lead/ScaleUP.Services.LeadModels/"]
RUN dotnet restore "./Services/Lead/ScaleUP.Services.LeadAPI/./ScaleUP.Services.LeadAPI.csproj"
COPY . .
WORKDIR "/src/Services/Lead/ScaleUP.Services.LeadAPI"
RUN dotnet build "./ScaleUP.Services.LeadAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.LeadAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.LeadAPI.dll"]