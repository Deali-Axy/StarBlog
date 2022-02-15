FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["demo/FileParseTest/FileParseTest.csproj", "FileParseTest/"]
RUN dotnet restore "demo/FileParseTest/FileParseTest.csproj"
COPY . .
WORKDIR "/src/FileParseTest"
RUN dotnet build "FileParseTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileParseTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileParseTest.dll"]
