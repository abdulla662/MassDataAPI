FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MassDataAPI.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENV DatabasePath=/data/massdata.db
ENV SeedCount=1000000
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "MassDataAPI.dll"]
