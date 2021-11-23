# hub.docker.com/_/microsoft-dotnet-sdk
# https://hub.docker.com/_/microsoft-dotnet-a

FROM mcr.microsoft.com/dotnet/runtime:6.0-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src

COPY ["/src/Rn.DnsUpdater/Rn.DnsUpdater.csproj", "Rn.DnsUpdater/"]

RUN dotnet restore "Rn.DnsUpdater/Rn.DnsUpdater.csproj"

COPY "/src/Rn.DnsUpdater/" "Rn.DnsUpdater/"

WORKDIR "/src/Rn.DnsUpdater/"

RUN dotnet build "Rn.DnsUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rn.DnsUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rn.DnsUpdater.dll"]
