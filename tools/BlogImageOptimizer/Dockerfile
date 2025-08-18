FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["demo/ImageTest/ImageTest.csproj", "ImageTest/"]
RUN dotnet restore "demo/ImageTest/ImageTest.csproj"
COPY . .
WORKDIR "/src/ImageTest"
RUN dotnet build "ImageTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageTest.dll"]
