FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AnalyticsService/AnalyticsService.Api/AnalyticsService.Api.csproj", "src/AnalyticsService/AnalyticsService.Api/"]
COPY ["src/AnalyticsService/AnalyticsService.Application/AnalyticsService.Application.csproj", "src/AnalyticsService/AnalyticsService.Application/"]
COPY ["src/AnalyticsService/AnalyticsService.Domain/AnalyticsService.Domain.csproj", "src/AnalyticsService/AnalyticsService.Domain/"]
COPY ["src/AnalyticsService/AnalyticsService.Infrastructure/AnalyticsService.Infrastructure.csproj", "src/AnalyticsService/AnalyticsService.Infrastructure/"]
COPY ["src/Shared/Shared.Api/Shared.Api.csproj", "src/Shared/Shared.Api/"]
COPY ["src/Shared/Shared.Auth/Shared.Auth.csproj", "src/Shared/Shared.Auth/"]
COPY ["src/Shared/Shared.Grpc.Contracts/Shared.Grpc.Contracts.csproj", "src/Shared/Shared.Grpc.Contracts/"]
COPY ["src/Shared/Shared.OpenTelemetry/Shared.OpenTelemetry.csproj", "src/Shared/Shared.OpenTelemetry/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["NuGet.config", "."]
RUN dotnet restore "src/AnalyticsService/AnalyticsService.Api/AnalyticsService.Api.csproj"
COPY . .
WORKDIR "/src/src/AnalyticsService/AnalyticsService.Api"
RUN dotnet build "./AnalyticsService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AnalyticsService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnalyticsService.Api.dll"]
