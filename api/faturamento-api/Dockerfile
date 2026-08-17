FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY faturamento-api/faturamento-api.csproj faturamento-api/
COPY Shared/Contracts/Contracts.csproj Shared/Contracts/
RUN dotnet restore faturamento-api/faturamento-api.csproj

COPY . .

RUN dotnet publish faturamento-api/faturamento-api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "faturamento-api.dll"]
