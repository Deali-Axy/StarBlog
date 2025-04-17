FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Ip2RegionDataProc/Ip2RegionDataProc.csproj", "src/Ip2RegionDataProc/"]
RUN dotnet restore "src/Ip2RegionDataProc/Ip2RegionDataProc.csproj"
COPY . .
WORKDIR "/src/src/Ip2RegionDataProc"
RUN dotnet build "Ip2RegionDataProc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ip2RegionDataProc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ip2RegionDataProc.dll"]
