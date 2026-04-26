FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AiChatService/AiChatService.Api/AiChatService.Api.csproj", "src/AiChatService/AiChatService.Api/"]
COPY ["src/AiChatService/AiChatService.Application/AiChatService.Application.csproj", "src/AiChatService/AiChatService.Application/"]
COPY ["src/AiChatService/AiChatService.Domain/AiChatService.Domain.csproj", "src/AiChatService/AiChatService.Domain/"]
COPY ["src/AiChatService/AiChatService.Infrastructure/AiChatService.Infrastructure.csproj", "src/AiChatService/AiChatService.Infrastructure/"]
COPY ["src/Shared/Shared.Api/Shared.Api.csproj", "src/Shared/Shared.Api/"]
COPY ["src/Shared/Shared.Auth/Shared.Auth.csproj", "src/Shared/Shared.Auth/"]
COPY ["src/Shared/Shared.Abstractions/Shared.Abstractions.csproj", "src/Shared/Shared.Abstractions/"]
COPY ["src/Shared/Shared.Caching/Shared.Caching.csproj", "src/Shared/Shared.Caching/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]
COPY ["src/Shared/Shared.Kernal/Shared.Kernal.csproj", "src/Shared/Shared.Kernal/"]
COPY ["src/Shared/Shared.OpenTelemetry/Shared.OpenTelemetry.csproj", "src/Shared/Shared.OpenTelemetry/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["NuGet.config", "."]
RUN dotnet restore "src/AiChatService/AiChatService.Api/AiChatService.Api.csproj"
COPY . .
WORKDIR "/src/src/AiChatService/AiChatService.Api"
RUN dotnet build "./AiChatService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AiChatService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AiChatService.Api.dll"]
