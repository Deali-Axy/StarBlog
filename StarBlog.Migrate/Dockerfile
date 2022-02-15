FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["StarBlog.Migrate/StarBlog.Migrate.csproj", "StarBlog.Migrate/"]
RUN dotnet restore "StarBlog.Migrate/StarBlog.Migrate.csproj"
COPY . .
WORKDIR "/src/StarBlog.Migrate"
RUN dotnet build "StarBlog.Migrate.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StarBlog.Migrate.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StarBlog.Migrate.dll"]
