FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GoogleLimitedInputAuth.Net.UsageExample/GoogleLimitedInputAuth.Net.UsageExample.csproj", "GoogleLimitedInputAuth.Net.UsageExample/"]
RUN dotnet restore "GoogleLimitedInputAuth.Net.UsageExample/GoogleLimitedInputAuth.Net.UsageExample.csproj"
COPY . .
WORKDIR "/src/GoogleLimitedInputAuth.Net.UsageExample"
RUN dotnet build "./GoogleLimitedInputAuth.Net.UsageExample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GoogleLimitedInputAuth.Net.UsageExample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoogleLimitedInputAuth.Net.UsageExample.dll"]
