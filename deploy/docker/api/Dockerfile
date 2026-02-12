FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App

COPY . ./
RUN dotnet restore src/presentation/api/Api.csproj

RUN dotnet publish src/presentation/api/Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build /App/out .

ENV DOTNET_URLS=http://+:5143
EXPOSE 5143

ENTRYPOINT ["dotnet", "Api.dll"]
