# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY GAM106ASM/*.csproj ./GAM106ASM/
RUN dotnet restore ./GAM106ASM/GAM106ASM.csproj

# Copy everything else and build
COPY GAM106ASM/. ./GAM106ASM/
WORKDIR /source/GAM106ASM
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

# Expose port (Fly.io uses 8080)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "GAM106ASM.dll"]
