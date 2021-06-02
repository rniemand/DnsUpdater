# docker build -t niemandr/rndnsupdater:dev-build .

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

COPY ["/src/Rn.DnsUpdater/Rn.DnsUpdater.csproj", "/rn-build/Rn.DnsUpdater/"]
RUN dotnet restore "/rn-build/Rn.DnsUpdater/Rn.DnsUpdater.csproj"

COPY /src/Rn.DnsUpdater/ /rn-build/Rn.DnsUpdater/

WORKDIR /rn-build/Rn.DnsUpdater/
RUN dotnet build "Rn.DnsUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rn.DnsUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rn.DnsUpdater.dll"]
