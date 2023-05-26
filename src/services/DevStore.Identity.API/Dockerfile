#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/services/DevStore.Identity.API/DevStore.Identity.API.csproj", "src/services/DevStore.Identity.API/"]
RUN dotnet restore "src/services/DevStore.Identity.API/DevStore.Identity.API.csproj"
COPY . .
WORKDIR "/src/src/services/DevStore.Identity.API"
RUN dotnet build "DevStore.Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DevStore.Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevStore.Identity.API.dll"]