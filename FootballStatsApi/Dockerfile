# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
# WORKDIR /app
# EXPOSE 80
# EXPOSE 443

# FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# WORKDIR /src
# COPY ["FootballStatsApi/FootballStatsApi.csproj", "FootballStatsApi/"]
# RUN dotnet restore "FootballStatsApi/FootballStatsApi.csproj"
# COPY . .
# WORKDIR "/src/FootballStatsApi"
# RUN dotnet build "FootballStatsApi.csproj" -c Release -o /app/build

# FROM build AS publish
# RUN dotnet publish "FootballStatsApi.csproj" -c Release -o /app/publish

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "FootballStatsApi.dll"]
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

CMD ASPNETCORE_URLS="http://*:$PORT" dotnet FootballStatsApi.dll

# # Get base SDK Image from Microsoft
# FROM mcr.microsoft.com/dotnet/core/sdk:6.0 AS build-env
# WORKDIR /app

# # Copy the CSPROJ file and restore any dependencies (via NUGET)]
# COPY *.csproj ./
# RUN dotnet restore

# # Copy the project files and build our release
# COPY . ./
# RUN dotnet publish -c Release -o out

# # Generate runtime image
# FROM mcr.microsoft.com/dotnet/core/aspnet:6.0
# WORKDIR /app
# EXPOSE 80
# COPY --from=build-env /app/out .
# ENTRYPOINT ["dotnet", "FootballStatsApi.dll"]