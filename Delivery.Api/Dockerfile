#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG BuildVersion
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN \
    apk --no-cache update \
    && apk --no-cache upgrade \
    && apk add --no-cache tzdata \
    && apk add --no-cache icu-libs bash

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV Version=${BuildVersion}

RUN echo "version number" ${BuildVersion}

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
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