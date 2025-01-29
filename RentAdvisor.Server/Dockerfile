# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install NodeJs
RUN apt-get -y update \
    && apt-get install -y curl \
    && curl -sL https://deb.nodesource.com/setup_18.x | bash - \ 
    && apt-get install -y nodejs \
    && apt-get clean

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["rentadvisor.client/nuget.config", "rentadvisor.client/"]
COPY ["RentAdvisor.Server/RentAdvisor.Server.csproj", "RentAdvisor.Server/"]
COPY ["rentadvisor.client/rentadvisor.client.esproj", "rentadvisor.client/"]
RUN dotnet restore "./RentAdvisor.Server/RentAdvisor.Server.csproj"
COPY . .
WORKDIR "/src/RentAdvisor.Server"
RUN dotnet build "./RentAdvisor.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RentAdvisor.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentAdvisor.Server.dll"]