FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
WORKDIR /app/web

RUN dotnet restore

# Copy everything else and build
RUN dotnet publish -c Release -o out

CMD rm -f /app/web/out/appsettings.json && rm -f /app/web/out/appsettings.Development.json    

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/web/out .
ENTRYPOINT ["dotnet", "web.dll"]
