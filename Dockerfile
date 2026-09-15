# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for efficient layer caching
COPY BackEnd/BackEnd/BackEnd.csproj BackEnd/BackEnd/
RUN dotnet restore BackEnd/BackEnd/BackEnd.csproj

# Copy remaining source files and build/publish
COPY BackEnd/BackEnd/ BackEnd/BackEnd/
WORKDIR /src/BackEnd/BackEnd
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Expose default HTTP port
EXPOSE 8080

# Environment variables for Render and container execution
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BackEnd.dll"]
