#Eigenes DockerFile:
#-----------------------------------------------

# Diese Stufe wird verwendet, wenn sie von VS im Schnellmodus ausgeführt wird (Standardeinstellung für Debugkonfiguration).
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 1: Build the Blazor Server app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Kopiere die .csproj-Datei und führe 'dotnet restore' aus
COPY QMK.TypeRush.ServerApp/QMK.TypeRush.ServerApp.csproj ./
RUN dotnet restore

# Kopiere den Rest des Projekts und baue die Anwendung
COPY . ./
RUN dotnet publish -c Release -o /out

# Stage 2: Erstelle das Laufzeit-Image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

# Definiere Umgebungsvariablen (optional)
ENV ASPNETCORE_ENVIRONMENT=Production

# Optional: Healthcheck hinzufügen
HEALTHCHECK CMD curl --fail http://localhost:80/health || exit 1

# Diese Stufe wird in der Produktion oder bei Ausführung von VS im regulären Modus verwendet (Standard, wenn die Debugkonfiguration nicht verwendet wird).
FROM base AS final
WORKDIR /app
COPY --from=runtime /app .
ENTRYPOINT ["dotnet", "QMK.TypeRush.ServerApp.dll"]