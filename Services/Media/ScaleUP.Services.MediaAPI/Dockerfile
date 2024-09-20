#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY Services/Media/ScaleUP.Services.MediaAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/Media/ScaleUP.Services.MediaAPI/https /app/https/
COPY Services/Media/ScaleUP.Services.MediaAPI/Rotativa/Linux/wkhtmltopdf /app/Rotativa/Linux/wkhtmltopdf
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
RUN chown app /app
RUN apt-get update -qq && apt-get -y install libgdiplus libc6-dev
USER app
WORKDIR /app
EXPOSE 5009
EXPOSE 7009

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/Media/ScaleUP.Services.MediaAPI/https /src/https/
COPY ["Services/Media/ScaleUP.Services.MediaAPI/ScaleUP.Services.MediaAPI.csproj", "Services/Media/ScaleUP.Services.MediaAPI/"]
COPY ["BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/ScaleUP.BuildingBlocks.GRPC.Contracts.csproj", "BuildingBlocks/GRPC/ScaleUP.BuildingBlocks.GRPC.Contracts/"]
COPY ["BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/ScaleUP.BuildingBlocks.Logging.csproj", "BuildingBlocks/Logging/ScaleUP.BuildingBlocks.Logging/"]
COPY ["Global/ScaleUP.Global.Infrastructure/ScaleUP.Global.Infrastructure.csproj", "Global/ScaleUP.Global.Infrastructure/"]
COPY ["Services/Media/ScaleUP.Services.MediaDTO/ScaleUP.Services.MediaDTO.csproj", "Services/Media/ScaleUP.Services.MediaDTO/"]
COPY ["Services/Media/ScaleUP.Services.MediaModels/ScaleUP.Services.MediaModels.csproj", "Services/Media/ScaleUP.Services.MediaModels/"]
RUN dotnet restore "./Services/Media/ScaleUP.Services.MediaAPI/./ScaleUP.Services.MediaAPI.csproj"
COPY . .
WORKDIR "/src/Services/Media/ScaleUP.Services.MediaAPI"
RUN dotnet build "./ScaleUP.Services.MediaAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.MediaAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#RUN chmod +x /app/Rotavita/Linux/wkhtmltopdf
ENTRYPOINT ["dotnet", "ScaleUP.Services.MediaAPI.dll"]