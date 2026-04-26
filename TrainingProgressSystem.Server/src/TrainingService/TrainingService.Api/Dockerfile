FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TrainingService/TrainingService.Api/TrainingService.Api.csproj", "src/TrainingService/TrainingService.Api/"]
COPY ["src/TrainingService/TrainingService.Application/TrainingService.Application.csproj", "src/TrainingService/TrainingService.Application/"]
COPY ["src/TrainingService/TrainingService.Domain/TrainingService.Domain.csproj", "src/TrainingService/TrainingService.Domain/"]
COPY ["src/TrainingService/TrainingService.Infrastructure/TrainingService.Infrastructure.csproj", "src/TrainingService/TrainingService.Infrastructure/"]
COPY ["src/Shared/Shared.Api/Shared.Api.csproj", "src/Shared/Shared.Api/"]
COPY ["src/Shared/Shared.Auth/Shared.Auth.csproj", "src/Shared/Shared.Auth/"]
COPY ["src/Shared/Shared.Grpc.Contracts/Shared.Grpc.Contracts.csproj", "src/Shared/Shared.Grpc.Contracts/"]
COPY ["src/Shared/Shared.OpenTelemetry/Shared.OpenTelemetry.csproj", "src/Shared/Shared.OpenTelemetry/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["NuGet.config", "."]
RUN dotnet restore "src/TrainingService/TrainingService.Api/TrainingService.Api.csproj"
COPY . .
WORKDIR "/src/src/TrainingService/TrainingService.Api"
RUN dotnet build "./TrainingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrainingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrainingService.Api.dll"]
