FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["DummyClient.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build "DummyClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DummyClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DummyClient.dll"]
