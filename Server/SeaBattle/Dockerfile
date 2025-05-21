FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY SeaBattle.csproj ./
RUN dotnet restore SeaBattle.csproj

COPY . .

RUN dotnet publish SeaBattle.csproj -c Release -o /app/publish

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SeaBattle.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeaBattle.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5183;https://+:8081
EXPOSE 5183
EXPOSE 8081

ENTRYPOINT ["dotnet", "SeaBattle.dll"]