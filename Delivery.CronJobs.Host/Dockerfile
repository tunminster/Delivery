ARG BuildVersion
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG BuildVersion

RUN \
    apk --no-cache update \
    && apk --no-cache upgrade \
    && apk add --no-cache tzdata \
    && apk add --no-cache icu-libs bash

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV Version=$BuildVersion

RUN addgroup -S hn --gid 1000 && adduser -S hnuser -G hn --uid 1000 \
    && mkdir -p /usr/src/app \
    && chown -R hnuser:hn /usr/ \
    

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["Delivery.CronJobs.Host/Delivery.CronJobs.Host.csproj", "Delivery.CronJobs.Host/"]
RUN dotnet restore "Delivery.CronJobs.Host/Delivery.CronJobs.Host.csproj"
COPY . .
WORKDIR "/src/Delivery.CronJobs.Host"
RUN dotnet build "Delivery.CronJobs.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Delivery.CronJobs.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Delivery.CronJobs.Host.dll"]
