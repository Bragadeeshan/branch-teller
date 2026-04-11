# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app

# Copy project files
COPY src/BranchTeller.Core/BranchTeller.Core.csproj src/BranchTeller.Core/
COPY src/BranchTeller.Api/BranchTeller.Api.csproj src/BranchTeller.Api/
COPY src/BranchTeller.Console/BranchTeller.Console.csproj src/BranchTeller.Console/

# Copy all source code
COPY src/ src/

# Restore dependencies
RUN dotnet restore src/BranchTeller.Api/BranchTeller.Api.csproj

# Publish the API in release mode
RUN dotnet publish src/BranchTeller.Api/BranchTeller.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/data

EXPOSE 8080

ENTRYPOINT ["dotnet", "BranchTeller.Api.dll"]