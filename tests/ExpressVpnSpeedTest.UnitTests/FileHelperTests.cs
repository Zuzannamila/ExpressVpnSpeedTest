using System.Text.Json;
using ExpressVpnSpeedTest.Utils;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpressVpnSpeedTestTests;

public class FileHelperTests : IDisposable
{
    private readonly IFileHelper _fileHelper;
    private readonly Mock<ILogger<FileHelper>> _loggerMock;
    private readonly List<string> _tempFiles = new();
    
    public FileHelperTests()
    {
        _loggerMock = new Mock<ILogger<FileHelper>>();
        _fileHelper = new FileHelper(_loggerMock.Object);
    }

    private string CreateTempJsonFile(string jsonContent)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, jsonContent);
        _tempFiles.Add(tempFile);
        return tempFile;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }

    #region ReadJsonFile
    [Fact]
    public void ReadJsonFile_ShouldReturnLocations_WhenJsonIsValid()
    {
        // Arrange
        var validJson = @"{
            ""locations"": [
                { ""country"": ""Poland"", ""city"": ""Warsaw"", ""ovpnConfigFile"": ""poland.ovpn"" },
                { ""country"": ""Ukraine"", ""city"": ""Kiev"", ""ovpnConfigFile"": ""ukraine.ovpn"" }
            ]
        }";
        var filePath = CreateTempJsonFile(validJson);

        // Act
        var result = _fileHelper.ReadJsonFile(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Locations);
        Assert.Equal(2, result.Locations.Count);
        Assert.Equal("Poland", result.Locations[0].Country);
        Assert.Equal("Warsaw", result.Locations[0].City);
        Assert.Equal("poland.ovpn", result.Locations[0].OvpnConfigFile);
    }
    [Fact]
    public void ReadJsonFile_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var path = "nonexistent.json";
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _fileHelper.ReadJsonFile(path));
    }
    [Fact]
    public void ReadJsonFile_ShouldThrowInvalidDataException_WhenFileisEmptyOrDataIsMissing()
    {
        // Arrange
        var emptyJson = "{ \"locations\": [] }";
        var path = CreateTempJsonFile(emptyJson);
        // Act & Assert
        Assert.Throws<InvalidDataException>(() => _fileHelper.ReadJsonFile(path));
    }
    [Fact]
    public void ReadJsonFile_ShouldThrowJsonException_WhenJsonIsInvalid()
    {
        // Arrange
        var invalidJson = "{invalid json}";
        var path = CreateTempJsonFile(invalidJson);

        // Act & Assert
        Assert.Throws<JsonException>(() => _fileHelper.ReadJsonFile(path));
    }

    #endregion

    #region WriteResultsToFileAsync
    [Fact]
    public async Task WriteResultsToFileAsync_ShouldSuccessfullyWriteJsonFile()
    {
        // Arrange
        var expectedOutput = new Output
        {
            MachineName = "test-machine",
            OS = "Linux",
            WithoutVPN = new SpeedTestResult
            {
                DownloadMbps = 90,
                UploadMbps = 94,
                PingMs = 9
            },
            VPNStats = new List<VPNStats>
            {
                new VPNStats
                {
                    LocationName = "Poznan, Poland",
                    TimeToConnect = "10.07 seconds",
                    VPNSpeed = new SpeedTestResult
                    {
                        DownloadMbps = 56,
                        UploadMbps = 73,
                        PingMs = 15
                    }
                },
                new VPNStats
                {
                    LocationName = "Kiev, Ukraine",
                    TimeToConnect = "10.01 seconds",
                    VPNSpeed = new SpeedTestResult
                    {
                        DownloadMbps = 44,
                        UploadMbps = 67,
                        PingMs = 28
                    }
                },
                new VPNStats
                {
                    LocationName = "Denver, United States",
                    TimeToConnect = "10.02 seconds",
                    VPNSpeed = new SpeedTestResult
                    {
                        DownloadMbps = 27,
                        UploadMbps = 51,
                        PingMs = 162
                    }
                }
            }
        };

         string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

        // Act
        await _fileHelper.WriteResultsToFileAsync(tempFile, expectedOutput);

        // Assert
        Assert.True(File.Exists(tempFile));

        string writtenJson = await File.ReadAllTextAsync(tempFile);
        var actualOutput = JsonSerializer.Deserialize<Output>(writtenJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(actualOutput);
        Assert.Equal(expectedOutput.MachineName, actualOutput.MachineName);
        Assert.Equal(expectedOutput.OS, actualOutput.OS);
        Assert.Equal(expectedOutput.WithoutVPN.DownloadMbps, actualOutput.WithoutVPN.DownloadMbps);
        Assert.Equal(expectedOutput.WithoutVPN.UploadMbps, actualOutput.WithoutVPN.UploadMbps);
        Assert.Equal(expectedOutput.WithoutVPN.PingMs, actualOutput.WithoutVPN.PingMs);
        Assert.Equal(expectedOutput.VPNStats.Count, actualOutput.VPNStats.Count);

        for (int i = 0; i < expectedOutput.VPNStats.Count; i++)
        {
            Assert.Equal(expectedOutput.VPNStats[i].LocationName, actualOutput.VPNStats[i].LocationName);
            Assert.Equal(expectedOutput.VPNStats[i].TimeToConnect, actualOutput.VPNStats[i].TimeToConnect);
            Assert.Equal(expectedOutput.VPNStats[i].VPNSpeed.DownloadMbps, actualOutput.VPNStats[i].VPNSpeed.DownloadMbps);
            Assert.Equal(expectedOutput.VPNStats[i].VPNSpeed.UploadMbps, actualOutput.VPNStats[i].VPNSpeed.UploadMbps);
            Assert.Equal(expectedOutput.VPNStats[i].VPNSpeed.PingMs, actualOutput.VPNStats[i].VPNSpeed.PingMs);
        }
    }
    #endregion
}