FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./src/ExpressVpnSpeedTest/ExpressVpnSpeedTest.csproj ExpressVpnSpeedTest/
COPY ./src/ExpressVpnSpeedTestLibrary/ExpressVpnSpeedTestLibrary.csproj ExpressVpnSpeedTestLibrary/
RUN dotnet restore ExpressVpnSpeedTest/ExpressVpnSpeedTest.csproj

COPY ./src/ExpressVpnSpeedTest/ ./ExpressVpnSpeedTest/
COPY ./src/ExpressVpnSpeedTestLibrary/ ./ExpressVpnSpeedTestLibrary/
WORKDIR /src/ExpressVpnSpeedTest
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Install openvpn, speedtest-cli
RUN apt-get update && apt-get install -y curl openvpn iproute2 iputils-ping procps && \
    curl -s https://packagecloud.io/install/repositories/ookla/speedtest-cli/script.deb.sh | bash && \
    apt-get install -y speedtest && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out ./

# Copy VPN configs
COPY docker/expressvpn/*.ovpn /vpn/
COPY docker/expressvpn/expressvpn.auth /vpn/expressvpn.auth

ENTRYPOINT ["dotnet", "ExpressVpnSpeedTest.dll"]
