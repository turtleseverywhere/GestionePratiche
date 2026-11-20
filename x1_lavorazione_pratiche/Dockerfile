#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
#nella 6 esponeva la 80, la 8 espone la 8080..
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["zscaler.cer", "/usr/local/share/ca-certificates/zscaler.crt"]
RUN update-ca-certificates

COPY ["x1_lavorazione_pratiche.csproj", "./"]
RUN dotnet restore "x1_lavorazione_pratiche.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "x1_lavorazione_pratiche.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "x1_lavorazione_pratiche.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "x1_lavorazione_pratiche.dll"]
#questo sotto va bene per aspettare che eventualmente salga il db.. con sql server
#serviva.. con postgres non ho mai avuto problemi
ENTRYPOINT ["/bin/sh", "-c", "echo 'Waiting 30s for db..' && sleep 30s && echo 'Waited 30s for db' && dotnet x1_lavorazione_pratiche.dll"]