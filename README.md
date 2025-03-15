# ExpressVpnSpeedTest
A console application built with .NET 8 that runs internet speed tests both with and without a VPN connection. This tool uses Ookla-cli for speed testing and the CliWrap library to simplify command-line interactions.

OVERVIEW

ExpressVpnSpeedTest is designed to:
Measure baseline internet speeds without VPN.
Connect to various VPN locations (using configuration files for OpenVPN).
Execute a series of speed tests (using Ookla-cli) to determine download, upload, and ping.
Record connection times and average speed results over five test iterations per location.
Due to limitations in running ExpressVPN CLI directly in Docker (systemd issues), the tool uses OpenVPN with configuration files. As tracking the exact connection time can be challenging in this environment, a fixed assumption of 10 seconds is used for connection timing.

WORKFLOW

Program Orchestration:
Program.cs reads an input.json file located at the root of the project.
The application orchestrates speed tests and VPN connections sequentially.

Speed Testing:
Utilizes Ookla-cli for running speed tests.
Leverages the CliWrap library as an abstraction over System.Diagnostics.Process for robust command-line interactions.
Performs 5 iterations of speed tests for each VPN configuration and calculates the average.

VPN Connection:
Instead of using ExpressVPN CLI, the project connects via OpenVPN using extended configuration files provided in the input.json.
Assumes a 10-second connection time due to system limitations within Docker.
Docker Environment:

The app is containerized, running on Debian (default for Microsoft.Sdk) to avoid Linux-specific configuration issues.

SETUP

docker run --rm -it \
  --cap-add=NET_ADMIN \
  --device /dev/net/tun \
  -v "$(pwd)/src/ExpressVpnSpeedTest/input.json:/app/input.json" \
  -v "$(pwd)/src/ExpressVpnSpeedTest/output:/app/output" \
  expressvpn-speedtest:1.0.0

INPUT

The utility accepts an input.json file with the following structure:
{
    "locations": [
      { "country": "Poland", "city": "Warsaw", "ovpnConfigFile": "my_expressvpn_poland_udp.ovpn" },
      { "country": "Ukraine", "city": "Kiev", "ovpnConfigFile": "my_expressvpn_ukraine_udp.ovpn" },
      { "country": "United States", "city": "Denver", "ovpnConfigFile": "my_expressvpn_usa_-_denver_udp.ovpn" }
    ]
}
  
OUTPUT

Output json file in in the output folder in the ExpressVpnSpeedtest console app root directory if ran with -v "$(pwd)/src/ExpressVpnSpeedTest/output:/app/output" \
{
  "MachineName": "17b497e7cf1c",
  "OS": "Unix 6.10.14.0",
  "WithoutVPN": {
    "DownloadMbps": 86,
    "UploadMbps": 90,
    "PingMs": 9
  },
  "VPNStats": [
    {
      "LocationName": "Warsaw, Poland",
      "TimeToConnect": "10.07 seconds",
      "VPNSpeed": {
        "DownloadMbps": 45,
        "UploadMbps": 58,
        "PingMs": 17
      }
    },
    {
      "LocationName": "Kiev, Ukraine",
      "TimeToConnect": "10.03 seconds",
      "VPNSpeed": {
        "DownloadMbps": 45,
        "UploadMbps": 62,
        "PingMs": 27
      }
    },
    {
      "LocationName": "Denver, United States",
      "TimeToConnect": "10.02 seconds",
      "VPNSpeed": {
        "DownloadMbps": 15,
        "UploadMbps": 46,
        "PingMs": 153
      }
    }
  ]
}

LOGGING

-  I implemented vast logging throughout the application:
-  info: Program[0]
      Test started
info: ExpressVpnSpeedTest.Utils.FileHelper[0]
      Succesfully read 3 locations from file: input.json
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 97.892648 Mbps, Upload speed: 93.5488 Mbps, Ping: 7.542 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 82.234896 Mbps, Upload speed: 87.32104 Mbps, Ping: 11.935 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 85.14568 Mbps, Upload speed: 90.935424 Mbps, Ping: 7.213 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 81.711256 Mbps, Upload speed: 91.73284 Mbps, Ping: 9.131 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 83.651376 Mbps, Upload speed: 84.25184 Mbps, Ping: 9.626 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Final avarage speedtest result: Download speed: 86 Mbps, Upload speed: 90 Mbps, Ping: 9 Mbps
info: ExpressVpnSpeedTest.Services.VpnSpeedTestService[0]
      Connected to VPN using config file my_expressvpn_poland_udp.ovpn, after assummed 10 seconds
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 48.673976 Mbps, Upload speed: 59.977576 Mbps, Ping: 26.144 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 48.260568 Mbps, Upload speed: 45.339344 Mbps, Ping: 13.853 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 36.882896 Mbps, Upload speed: 66.096504 Mbps, Ping: 15.063 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 46.244624 Mbps, Upload speed: 63.926064 Mbps, Ping: 15.185 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 46.8832 Mbps, Upload speed: 56.029744 Mbps, Ping: 12.925 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Final avarage speedtest result: Download speed: 45 Mbps, Upload speed: 58 Mbps, Ping: 17 Mbps
info: ExpressVpnSpeedTest.Services.VpnSpeedTestService[0]
      VPN disconnected.
info: Program[0]
      Average speed test result for Warsaw, Poland: Download speed: 45 Mbps, Upload speed: 58 Mbps, Ping: 17 Mbps
info: ExpressVpnSpeedTest.Services.VpnSpeedTestService[0]
      Connected to VPN using config file my_expressvpn_ukraine_udp.ovpn, after assummed 10 seconds
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 37.995648 Mbps, Upload speed: 64.64424 Mbps, Ping: 27.432 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 39.761048 Mbps, Upload speed: 63.607504 Mbps, Ping: 25.787 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 36.607472 Mbps, Upload speed: 66.159208 Mbps, Ping: 30.255 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 37.792984 Mbps, Upload speed: 60.59916 Mbps, Ping: 26.37 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 75.140848 Mbps, Upload speed: 57.130968 Mbps, Ping: 26.081 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Final avarage speedtest result: Download speed: 45 Mbps, Upload speed: 62 Mbps, Ping: 27 Mbps
info: ExpressVpnSpeedTest.Services.VpnSpeedTestService[0]
      VPN disconnected.
info: Program[0]
      Average speed test result for Kiev, Ukraine: Download speed: 45 Mbps, Upload speed: 62 Mbps, Ping: 27 Mbps
info: ExpressVpnSpeedTest.Services.VpnSpeedTestService[0]
      Connected to VPN using config file my_expressvpn_usa_-_denver_udp.ovpn, after assummed 10 seconds
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 21.658824 Mbps, Upload speed: 49.34072 Mbps, Ping: 152.648 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 12.409024 Mbps, Upload speed: 50.700456 Mbps, Ping: 154.072 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 12.41012 Mbps, Upload speed: 50.35772 Mbps, Ping: 151.403 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 13.220224 Mbps, Upload speed: 39.165192 Mbps, Ping: 152.646 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Speedtest result: Download speed: 15.063472 Mbps, Upload speed: 42.11276 Mbps, Ping: 155.213 Mbps
info: ExpressVpnSpeedTest.Services.BaseTestSpeedService[0]
      Final avarage speedtest result: Download speed: 15 Mbps, Upload speed: 46 Mbps, Ping: 153 Mbps
info: ExpressVpnSpeedTest.Services.VpnSpeedTestService[0]
      VPN disconnected.
info: Program[0]
      Average speed test result for Denver, United States: Download speed: 15 Mbps, Upload speed: 46 Mbps, Ping: 153 Mbps
info: ExpressVpnSpeedTest.Utils.FileHelper[0]
      Successfully wrote results to file: /app/output/output.json

  
