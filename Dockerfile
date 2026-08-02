# ==========================
# Build
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

COPY . .

RUN dotnet restore src/AuthenticationService.API/AuthenticationService.API.csproj

RUN dotnet build src/AuthenticationService.API/AuthenticationService.API.csproj \
    -c Release \
    --no-restore

RUN dotnet publish src/AuthenticationService.API/AuthenticationService.API.csproj \
    -c Release \
    --no-build \
    -o /app/publish

# ==========================
# Runtime
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthenticationService.API.dll"]