#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/services/DS.Identity.API/DS.Identity.API.csproj", "src/services/DS.Identity.API/"]
RUN dotnet restore "src/services/DS.Identity.API/DS.Identity.API.csproj"
COPY . .
WORKDIR "/src/src/services/DS.Identity.API"
RUN dotnet build "DS.Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DS.Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DS.Identity.API.dll"]