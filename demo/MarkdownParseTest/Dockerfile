FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["demo/MarkdownParseTest/MarkdownParseTest.csproj", "MarkdownParseTest/"]
RUN dotnet restore "demo/MarkdownParseTest/MarkdownParseTest.csproj"
COPY . .
WORKDIR "/src/MarkdownParseTest"
RUN dotnet build "MarkdownParseTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarkdownParseTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarkdownParseTest.dll"]
