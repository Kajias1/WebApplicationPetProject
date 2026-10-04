# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WebApplication1/WebApplication1.csproj WebApplication1/
RUN dotnet restore WebApplication1/WebApplication1.csproj

COPY WebApplication1/ WebApplication1/

RUN dotnet publish WebApplication1/WebApplication1.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# ---- Migration stage ----
FROM build AS migration

RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src/WebApplication1

ENTRYPOINT ["dotnet", "ef", "database", "update"]


# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
