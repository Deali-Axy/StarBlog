FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["apps/web-legacy/StarBlog.Web.csproj", "apps/web-legacy/"]
COPY ["apps/api/src/StarBlog.Application/StarBlog.Application.csproj", "apps/api/src/StarBlog.Application/"]
COPY ["apps/api/src/StarBlog.Content/StarBlog.Content.csproj", "apps/api/src/StarBlog.Content/"]
COPY ["apps/api/src/StarBlog.Data/StarBlog.Data.csproj", "apps/api/src/StarBlog.Data/"]
COPY ["apps/api/src/StarBlog.Infrastructure/StarBlog.Infrastructure.csproj", "apps/api/src/StarBlog.Infrastructure/"]
RUN dotnet restore "apps/web-legacy/StarBlog.Web.csproj"
COPY . .
WORKDIR "/src/apps/web-legacy"
RUN dotnet build "StarBlog.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StarBlog.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StarBlog.Web.dll"]
