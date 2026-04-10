# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["VanThi/VanThi_.csproj", "VanThi/"]
RUN dotnet restore "VanThi/VanThi_.csproj"

# Copy everything else and build
COPY ["VanThi/", "VanThi/"]
WORKDIR "/src/VanThi"
RUN dotnet build "VanThi_.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "VanThi_.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
# Alternate between runtime and aspnet depending on needs, but usually aspnet for Web APIs
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Use the PORT environment variable provided by Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "VanThi_.dll"]
