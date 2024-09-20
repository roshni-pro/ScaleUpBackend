#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY Services/NBFC/ScaleUP.Services.NBFCAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/NBFC/ScaleUP.Services.NBFCAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates

USER app
WORKDIR /app
EXPOSE 5010
EXPOSE 7010

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/NBFC/ScaleUP.Services.NBFCAPI/https /src/https/
COPY ["Services/NBFC/ScaleUP.Services.NBFCAPI/ScaleUP.Services.NBFCAPI.csproj", "Services/NBFC/ScaleUP.Services.NBFCAPI/"]
RUN dotnet restore "./Services/NBFC/ScaleUP.Services.NBFCAPI/./ScaleUP.Services.NBFCAPI.csproj"
COPY . .
WORKDIR "/src/Services/NBFC/ScaleUP.Services.NBFCAPI"
RUN dotnet build "./ScaleUP.Services.NBFCAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.NBFCAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.NBFCAPI.dll"]