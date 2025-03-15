using ExpressVpnSpeedTest.Interfaces;
using ExpressVpnSpeedTest.Services;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace ExpressVpnSpeedTest.UnitTests
{

    public class VpnSpeedTestServiceTests
    {
        private readonly Mock<IBaseTestSpeedService> _baseTestSpeedServiceMock;
        private readonly Mock<ILogger<VpnSpeedTestService>> _loggerMock;
        private readonly Mock<VpnSpeedTestService> _vpnSpeedTestServiceMock;

        public VpnSpeedTestServiceTests()
        {
            _baseTestSpeedServiceMock = new Mock<IBaseTestSpeedService>();
            _loggerMock = new Mock<ILogger<VpnSpeedTestService>>();

            _vpnSpeedTestServiceMock = new Mock<VpnSpeedTestService>(_baseTestSpeedServiceMock.Object, _loggerMock.Object)
            {
                CallBase = true
            };
        }

        [Fact]
        public async Task RunVpnSpeedTestsAsync_ShouldReturnVPNStats_WhenSuccessful()
        {
            // Arrange
            var location = new Location { City = "Warsaw", Country = "Poland", OvpnConfigFile = "test.ovpn" };
            double fakeConnectTime = 5.0;
            var expectedSpeedTestResult = new SpeedTestResult { DownloadMbps = 100, UploadMbps = 50, PingMs = 20 };

            _vpnSpeedTestServiceMock.Protected()
                .Setup<Task<double>>("ConnectVpnAsync", ItExpr.IsAny<string>())
                .ReturnsAsync(fakeConnectTime);
            _vpnSpeedTestServiceMock.Protected()
                .Setup<Task>("DisconnectVpnAsync")
                .Returns(Task.CompletedTask);

            _baseTestSpeedServiceMock
                .Setup(x => x.RunSpeedTestsAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedSpeedTestResult);

            // Act
            VPNStats vpnStats = await _vpnSpeedTestServiceMock.Object.RunVpnSpeedTestsAsync(location, 3);

            // Assert
            Assert.NotNull(vpnStats);
            Assert.Equal(location.ToString(), vpnStats.LocationName);
            Assert.Equal($"{fakeConnectTime.ToString("F2")} seconds", vpnStats.TimeToConnect);
            Assert.Equal(expectedSpeedTestResult, vpnStats.VPNSpeed);

            // Verify that DisconnectVpnAsync was called
            _vpnSpeedTestServiceMock.Protected().Verify("DisconnectVpnAsync", Times.Once());
        }

        [Fact]
        public async Task RunVpnSpeedTestsAsync_ShouldCallDisconnectVpnAsync_WhenRunSpeedTestsAsyncThrows()
        {
            // Arrange
            var location = new Location { City = "Warsaw", Country = "Poland", OvpnConfigFile = "test.ovpn" };
            double fakeConnectTime = 5.0;

            _vpnSpeedTestServiceMock.Protected()
                .Setup<Task<double>>("ConnectVpnAsync", ItExpr.IsAny<string>())
                .ReturnsAsync(fakeConnectTime);
            _vpnSpeedTestServiceMock.Protected()
                .Setup<Task>("DisconnectVpnAsync")
                .Returns(Task.CompletedTask);

            _baseTestSpeedServiceMock
                .Setup(x => x.RunSpeedTestsAsync(It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Simulated speed test failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _vpnSpeedTestServiceMock.Object.RunVpnSpeedTestsAsync(location, 3));

            // Ensure DisconnectVpnAsync was called even when an exception occurs
            _vpnSpeedTestServiceMock.Protected().Verify("DisconnectVpnAsync", Times.Once());
        }
    }
}
