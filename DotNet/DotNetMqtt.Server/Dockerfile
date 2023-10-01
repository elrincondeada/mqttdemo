# Utilizar la imagen base de .NET
FROM mcr.microsoft.com/dotnet/aspnet:latest AS base
WORKDIR /app
EXPOSE 1883

# Compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src
COPY ["DotNetMqtt.Server.csproj", "/src"]
RUN dotnet restore "DotNetMqtt.Server.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "DotNetMqtt.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotNetMqtt.Server.csproj" -c Release -o /app/publish

# Copiar y ejecutar la aplicación
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotNetMqtt.Server.dll"]
