FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["apps/api/src/StarBlog.Api/StarBlog.Api.csproj", "apps/api/src/StarBlog.Api/"]
RUN dotnet restore "apps/api/src/StarBlog.Api/StarBlog.Api.csproj"
COPY . .
WORKDIR "/src/apps/api/src/StarBlog.Api"
RUN dotnet build "StarBlog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StarBlog.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StarBlog.Api.dll"]
