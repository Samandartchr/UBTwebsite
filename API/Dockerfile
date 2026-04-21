# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
# Ensure dependencies are restored and app is published
RUN dotnet publish -c Release -o /app

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# CRITICAL: Ensure the secret file is available in the working directory
# If secret.json is in your root folder, the 'COPY . .' above handles it, 
# but explicitly copying it ensures it's in the /app folder at runtime.
COPY appsettings.json .
COPY secret.json . 

ENTRYPOINT ["dotnet", "API.dll"]