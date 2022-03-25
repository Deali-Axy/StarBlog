FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["demo/RegTest/RegTest.csproj", "RegTest/"]
RUN dotnet restore "demo/RegTest/RegTest.csproj"
COPY . .
WORKDIR "/src/RegTest"
RUN dotnet build "RegTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RegTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RegTest.dll"]
