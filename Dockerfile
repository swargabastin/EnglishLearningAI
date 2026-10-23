# Multi-stage build for ASP.NET Core 8 Web API
# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for efficient Docker layer caching
COPY ["backend/EnglishLearning.Domain/EnglishLearning.Domain.csproj", "backend/EnglishLearning.Domain/"]
COPY ["backend/EnglishLearning.Application/EnglishLearning.Application.csproj", "backend/EnglishLearning.Application/"]
COPY ["backend/EnglishLearning.Infrastructure/EnglishLearning.Infrastructure.csproj", "backend/EnglishLearning.Infrastructure/"]
COPY ["backend/EnglishLearning.Api/EnglishLearning.Api.csproj", "backend/EnglishLearning.Api/"]

# Restore dependencies
RUN dotnet restore "backend/EnglishLearning.Api/EnglishLearning.Api.csproj"

# Copy all backend source code
COPY backend/ backend/

# Build and publish release binaries
WORKDIR "/src/backend/EnglishLearning.Api"
RUN dotnet publish "EnglishLearning.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Ensure uploads directory structure exists
RUN mkdir -p /app/uploads/avatars

# Copy published application from build stage
COPY --from=build /app/publish .

# Configure port and environment defaults
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EnglishLearning.Api.dll"]
