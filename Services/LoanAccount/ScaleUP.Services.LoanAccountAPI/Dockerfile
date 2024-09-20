#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
COPY Services/LoanAccount/ScaleUP.Services.LoanAccountAPI/https/scaleupcertdev.crt /usr/local/share/ca-certificates/scaleupcertdev.crt
COPY Services/LoanAccount/ScaleUP.Services.LoanAccountAPI/https /app/https/
#RUN sudo chmod 644 /etc/ca-certificates.conf
#RUN sudo echo scaleupcertdev.crt >> /etc/ca-certificates.conf
RUN chmod 644 /usr/local/share/ca-certificates/scaleupcertdev.crt && update-ca-certificates
USER app
WORKDIR /app
EXPOSE 5011
EXPOSE 7011

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Services/LoanAccount/ScaleUP.Services.LoanAccountAPI/https /src/https/
COPY ["Services/LoanAccount/ScaleUP.Services.LoanAccountAPI/ScaleUP.Services.LoanAccountAPI.csproj", "Services/LoanAccount/ScaleUP.Services.LoanAccountAPI/"]
RUN dotnet restore "./Services/LoanAccount/ScaleUP.Services.LoanAccountAPI/ScaleUP.Services.LoanAccountAPI.csproj"
COPY . .
WORKDIR "/src/Services/LoanAccount/ScaleUP.Services.LoanAccountAPI"
RUN dotnet build "./ScaleUP.Services.LoanAccountAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScaleUP.Services.LoanAccountAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScaleUP.Services.LoanAccountAPI.dll"]