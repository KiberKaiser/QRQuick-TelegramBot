FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libfontconfig1 \
        libgtk-3-0 \
        libx11-6 \
        libpango-1.0-0 \
        libpangocairo-1.0-0 \
        libgdiplus

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .

RUN dotnet restore "QRCodeGeneratorBot.csproj"
RUN dotnet build "QRCodeGeneratorBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QRCodeGeneratorBot.csproj" \
    -c $BUILD_CONFIGURATION \
    -r linux-x64 \
    --self-contained true \
    -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["./QRCodeGeneratorBot"]
