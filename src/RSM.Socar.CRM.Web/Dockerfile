# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RSM.Socar.CRM.Web/RSM.Socar.CRM.Web.csproj", "src/RSM.Socar.CRM.Web/"]
COPY ["src/RSM.Socar.CRM.Application/RSM.Socar.CRM.Application.csproj", "src/RSM.Socar.CRM.Application/"]
COPY ["src/RSM.Socar.CRM.Domain/RSM.Socar.CRM.Domain.csproj", "src/RSM.Socar.CRM.Domain/"]
COPY ["src/RSM.Socar.CRM.Infrastructure/RSM.Socar.CRM.Infrastructure.csproj", "src/RSM.Socar.CRM.Infrastructure/"]
COPY ["src/RSM.Socar.CRM.ServiceDefaults/RSM.Socar.CRM.ServiceDefaults.csproj", "src/RSM.Socar.CRM.ServiceDefaults/"]
RUN dotnet restore "./src/RSM.Socar.CRM.Web/RSM.Socar.CRM.Web.csproj"
COPY . .
WORKDIR "/src/src/RSM.Socar.CRM.Web"
RUN dotnet build "./RSM.Socar.CRM.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RSM.Socar.CRM.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RSM.Socar.CRM.Web.dll"]