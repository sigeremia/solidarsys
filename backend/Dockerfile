FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/ ./backend/
RUN dotnet restore backend/SolidarSYS.API/SolidarSYS.API.csproj
RUN dotnet publish backend/SolidarSYS.API/SolidarSYS.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "exec dotnet SolidarSYS.API.dll --urls http://0.0.0.0:${PORT:-10000}"]