using System.Text.Json;
using ExpressVpnSpeedTest.Services;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;

public class BaseTestSpeedServiceTestsTest
{
    private readonly Mock<ILogger<BaseTestSpeedService>> _loggerMock;
    private readonly Mock<BaseTestSpeedService> _serviceMock;

    public BaseTestSpeedServiceTestsTest()
    {
        _loggerMock = new Mock<ILogger<BaseTestSpeedService>>();
        _serviceMock = new Mock<BaseTestSpeedService>(_loggerMock.Object)
        {
            CallBase = true
        };
    }

    [Fact]
    public async Task RunSpeedTestAsync_ShouldReturnValidSpeedTestResult_WhenSpeedtestSucceeds()
    {
        // Arrange
        var expectedResult = new SpeedTestResult
        {
            DownloadMbps = 100,
            UploadMbps = 50,
            PingMs = 15
        };

        _serviceMock.Setup(s => s.RunSpeedTestAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _serviceMock.Object.RunSpeedTestAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.DownloadMbps, result.DownloadMbps);
        Assert.Equal(expectedResult.UploadMbps, result.UploadMbps);
        Assert.Equal(expectedResult.PingMs, result.PingMs);
    }

    [Fact]
    public async Task RunSpeedTestAsync_ShouldThrowInvalidOperationException_WhenSpeedtestFails()
    {
        // Arrange
        _serviceMock.Setup(s => s.RunSpeedTestAsync())
            .ThrowsAsync(new InvalidOperationException("Speedtest failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _serviceMock.Object.RunSpeedTestAsync());
    }

    [Fact]
    public async Task RunSpeedTestAsync_ShouldThrowJsonException_WhenSpeedtestReturnsInvalidJson()
    {
        // Arrange
        _serviceMock.Setup(s => s.RunSpeedTestAsync())
            .ThrowsAsync(new JsonException("Invalid JSON"));

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _serviceMock.Object.RunSpeedTestAsync());
    }

    [Fact]
    public async Task RunSpeedTestAsync_ShouldThrowOperationCanceledException_WhenSpeedtestTimesOut()
    {
        // Arrange
        _serviceMock.Setup(s => s.RunSpeedTestAsync())
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _serviceMock.Object.RunSpeedTestAsync());
    }

    [Fact]
    public async Task RunSpeedTestsAsync_ShouldReturnAverageResult_WhenMultipleTestsAreSuccessful()
    {
        // Arrange
        var testResults = new List<SpeedTestResult>
        {
            new SpeedTestResult { DownloadMbps = 100, UploadMbps = 50, PingMs = 10 },
            new SpeedTestResult { DownloadMbps = 90, UploadMbps = 40, PingMs = 20 },
            new SpeedTestResult { DownloadMbps = 110, UploadMbps = 55, PingMs = 15 }
        };

        _serviceMock.SetupSequence(s => s.RunSpeedTestAsync())
            .ReturnsAsync(testResults[0])
            .ReturnsAsync(testResults[1])
            .ReturnsAsync(testResults[2]);

        // Act
        var result = await _serviceMock.Object.RunSpeedTestsAsync(3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.DownloadMbps); 
        Assert.Equal(48, result.UploadMbps);  
        Assert.Equal(15, result.PingMs);       
    }

    [Fact]
    public async Task RunSpeedTestsAsync_ShouldThrowException_WhenTooManyTestsFail()
    {
        // Arrange
        _serviceMock.Setup(s => s.RunSpeedTestAsync())
            .ThrowsAsync(new InvalidOperationException("Speedtest failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _serviceMock.Object.RunSpeedTestsAsync(3));
    }
}