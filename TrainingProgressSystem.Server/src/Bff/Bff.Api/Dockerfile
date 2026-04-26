FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Bff/Bff.Api/Bff.Api.csproj", "src/Bff/Bff.Api/"]
COPY ["src/Bff/Bff.Application/Bff.Application.csproj", "src/Bff/Bff.Application/"]
COPY ["src/Bff/Bff.Domain/Bff.Domain.csproj", "src/Bff/Bff.Domain/"]
COPY ["src/Bff/Bff.Infrastructure/Bff.Infrastructure.csproj", "src/Bff/Bff.Infrastructure/"]
COPY ["src/Shared/Shared.Api/Shared.Api.csproj", "src/Shared/Shared.Api/"]
COPY ["src/Shared/Shared.Kernal/Shared.Kernal.csproj", "src/Shared/Shared.Kernal/"]
COPY ["src/Shared/Shared.OpenTelemetry/Shared.OpenTelemetry.csproj", "src/Shared/Shared.OpenTelemetry/"]
COPY ["src/Shared/Shared.Saga/Shared.Saga.csproj", "src/Shared/Shared.Saga/"]
COPY ["src/Shared/Shared.Abstractions/Shared.Abstractions.csproj", "src/Shared/Shared.Abstractions/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Grpc.Contracts/Shared.Grpc.Contracts.csproj", "src/Shared/Shared.Grpc.Contracts/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["NuGet.config", "."]
RUN dotnet restore "src/Bff/Bff.Api/Bff.Api.csproj"
COPY . .
WORKDIR "/src/src/Bff/Bff.Api"
RUN dotnet build "./Bff.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bff.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bff.Api.dll"]
