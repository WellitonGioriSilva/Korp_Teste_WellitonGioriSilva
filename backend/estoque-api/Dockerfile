FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY estoque-api/estoque-api.csproj estoque-api/
COPY Shared/Contracts/Contracts.csproj Shared/Contracts/
RUN dotnet restore estoque-api/estoque-api.csproj

COPY . .

RUN dotnet publish estoque-api/estoque-api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "estoque-api.dll"]
