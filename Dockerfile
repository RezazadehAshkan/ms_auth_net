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

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet ef migrations bundle \
    --project src/AuthenticationService.Infrastructure \
    --startup-project src/AuthenticationService.API \
    --self-contained \
    --target-runtime linux-x64 \
    --output /app/efbundle
# ==========================
# Runtime
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
 
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /app/efbundle ./efbundle

RUN chmod +x ./efbundle

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthenticationService.API.dll"]