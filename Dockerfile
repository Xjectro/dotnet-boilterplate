FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App

COPY . ./
RUN dotnet restore

RUN dotnet publish src/Api/Api.csproj -c Release -o out -p:EnvironmentName=Docker

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build /App/out .

ENV DOTNET_URLS=http://+:5143
EXPOSE 5143

ENTRYPOINT ["dotnet", "Api.dll"]
