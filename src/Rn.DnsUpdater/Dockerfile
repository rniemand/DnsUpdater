#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Rn.DnsUpdater/Rn.DnsUpdater.csproj", "Rn.DnsUpdater/"]
RUN dotnet restore "Rn.DnsUpdater/Rn.DnsUpdater.csproj"
COPY . .
WORKDIR "/src/Rn.DnsUpdater"
RUN dotnet build "Rn.DnsUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rn.DnsUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rn.DnsUpdater.dll"]