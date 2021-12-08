FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Delivery.Drivers.Host/Delivery.Drivers.Host.csproj", "Delivery.Drivers.Host/"]
RUN dotnet restore "Delivery.Drivers.Host/Delivery.Drivers.Host.csproj"
COPY . .
WORKDIR "/src/Delivery.Drivers.Host"
RUN dotnet build "Delivery.Drivers.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Delivery.Drivers.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Delivery.Drivers.Host.dll"]
