#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Delivery.Api/Delivery.Api.csproj", "Delivery.Api/"]
RUN dotnet restore "Delivery.Api/Delivery.Api.csproj"
COPY . .
WORKDIR "/src/Delivery.Api"
RUN dotnet build "Delivery.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Delivery.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Delivery.Api.dll"]