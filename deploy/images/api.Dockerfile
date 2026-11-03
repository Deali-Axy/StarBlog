FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:de0ee358cdd672f7017a003bed21e9ac9ed48a27c922a59b18175b6e82fa7fee
WORKDIR /app
COPY --chown=1654:1654 api/ ./
USER 1654:1654
EXPOSE 8080
ENTRYPOINT ["dotnet", "StarBlog.Api.dll"]
