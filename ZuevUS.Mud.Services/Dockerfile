# Base image with ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 6010

# Build stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZuevUS.Mud.Services/ZuevUS.Mud.Services.csproj", "ZuevUS.Mud.Services/"]
RUN dotnet restore "./ZuevUS.Mud.Services/ZuevUS.Mud.Services.csproj"
COPY . .
WORKDIR "/src/ZuevUS.Mud.Services"
RUN dotnet build "./ZuevUS.Mud.Services.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ZuevUS.Mud.Services.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZuevUS.Mud.Services.dll"]