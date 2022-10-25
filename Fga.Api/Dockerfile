FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Fga.Api/Fga.Api.csproj", "Fga.Api/"]
RUN dotnet restore "Fga.Api/Fga.Api.csproj"
COPY . .
WORKDIR "/src/Fga.Api"
RUN dotnet build "Fga.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fga.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fga.Api.dll"]
