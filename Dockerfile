# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["MedicalSuppliesCatalog.Lab06.csproj", "./"]
RUN dotnet restore "MedicalSuppliesCatalog.Lab06.csproj"

# Copy all source files and compile
COPY . .
RUN dotnet build "MedicalSuppliesCatalog.Lab06.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "MedicalSuppliesCatalog.Lab06.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose HTTP port 80 (standard for containerized web apps)
EXPOSE 80

# Environment configurations
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Command to run the application
ENTRYPOINT ["dotnet", "MedicalSuppliesCatalog.Lab06.dll"]
