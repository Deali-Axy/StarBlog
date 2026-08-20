FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["tools/DataProc/DataProc.csproj", "tools/DataProc/"]
COPY ["apps/api/src/StarBlog.Data/StarBlog.Data.csproj", "apps/api/src/StarBlog.Data/"]
COPY ["apps/api/src/StarBlog.Infrastructure/StarBlog.Infrastructure.csproj", "apps/api/src/StarBlog.Infrastructure/"]
RUN dotnet restore "tools/DataProc/DataProc.csproj"
COPY . .
WORKDIR "/src/tools/DataProc"
RUN dotnet build "DataProc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataProc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataProc.dll"]
