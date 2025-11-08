FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5142
EXPOSE 5142
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV SQLITE_DB_PATH=/app/catalog.db
ENTRYPOINT ["dotnet", "Catalog_Service.dll"]