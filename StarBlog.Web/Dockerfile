FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["StarBlog.Web/StarBlog.Web.csproj", "StarBlog.Web/"]
RUN dotnet restore "StarBlog.Web/StarBlog.Web.csproj"
COPY . .
WORKDIR "/src/StarBlog.Web"
RUN dotnet build "StarBlog.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StarBlog.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StarBlog.Web.dll"]
