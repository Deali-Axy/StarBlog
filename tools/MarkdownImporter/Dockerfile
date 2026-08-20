FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["tools/MarkdownImporter/MarkdownImporter.csproj", "tools/MarkdownImporter/"]
COPY ["apps/api/src/StarBlog.Data/StarBlog.Data.csproj", "apps/api/src/StarBlog.Data/"]
COPY ["apps/api/src/StarBlog.Content/StarBlog.Content.csproj", "apps/api/src/StarBlog.Content/"]
RUN dotnet restore "tools/MarkdownImporter/MarkdownImporter.csproj"
COPY . .
WORKDIR "/src/tools/MarkdownImporter"
RUN dotnet build "MarkdownImporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarkdownImporter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StarBlog.Tools.MarkdownImporter.dll"]
