#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY Services/Product/ScaleUP.Services.ProductAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/Product/ScaleUP.Services.ProductAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
USER app
WORKDIR /app
EXPOSE 5001
EXPOSE 7001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/Product/ScaleUP.Services.ProductAPI/https /src/https/
COPY ["Services/Product/ScaleUP.Services.ProductAPI/ScaleUP.Services.ProductAPI.csproj", "Services/Product/ScaleUP.Services.ProductAPI/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
COPY ["Services/Product/ScaleUP.Services.ProductDTO/ScaleUP.Services.ProductDTO.csproj", "Services/Product/ScaleUP.Services.ProductDTO/"]
COPY ["Services/Product/ScaleUP.Services.ProductModels/ScaleUP.Services.ProductModels.csproj", "Services/Product/ScaleUP.Services.ProductModels/"]
RUN dotnet restore "./Services/Product/ScaleUP.Services.ProductAPI/./ScaleUP.Services.ProductAPI.csproj"
COPY . .
WORKDIR "/src/Services/Product/ScaleUP.Services.ProductAPI"
RUN dotnet build "./ScaleUP.Services.ProductAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build
#COPY scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
#RUN echo scaleupcertdev.crt >> /etc/ca-certificates.conf
#RUN sudo chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.ProductAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.ProductAPI.dll"]
#COPY scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
#RUN echo scaleupcertdev.crt >> /etc/ca-certificates.conf
#RUN sudo chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates