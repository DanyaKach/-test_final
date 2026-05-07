FROM mcr.microsoft.com/dotnet/sdk:10.0 as build
WORKDIR /src

COPY ["WeatherStation.Api/WeatherStation.Api.csproj", "WeatherStation.Api/"]
COPY ["WeatherStation.sln", "."]
RUN dotnet restore "WeatherStation.sln"

COPY . .
WORKDIR "/src/WeatherStation.Api"
RUN dotnet build "WeatherStation.Api.csproj" -c Release -o /app/build

FROM build as publish
RUN dotnet publish "WeatherStation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000
EXPOSE 5001

ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "WeatherStation.Api.dll"]
