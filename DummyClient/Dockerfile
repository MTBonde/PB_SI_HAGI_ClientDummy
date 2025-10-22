FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ClientDummy.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build "ClientDummy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClientDummy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientDummy.dll"]
