FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["tools/BlogImageOptimizer/BlogImageOptimizer.csproj", "tools/BlogImageOptimizer/"]
RUN dotnet restore "tools/BlogImageOptimizer/BlogImageOptimizer.csproj"
COPY . .
WORKDIR "/src/tools/BlogImageOptimizer"
RUN dotnet build "BlogImageOptimizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogImageOptimizer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogImageOptimizer.dll"]
