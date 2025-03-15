using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ExpressVpnSpeedTest.Interfaces;
using ExpressVpnSpeedTest.Services;
using ExpressVpnSpeedTest.Utils;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ExpressVpnSpeedTest.IntegrationTests
{
    public class IntegrationTests
    {
        [Fact]
        public async Task RunFullIntegrationTest_Simulated()
        {
            string tempInputFile = Path.Combine(Path.GetTempPath(), "input.json");
            string tempOutputFile = Path.Combine(Path.GetTempPath(), "output.json");

            var sampleInput = new
            {
                Locations = new[]
                {
                    new { Country = "USA", City = "New York", OvpnConfigFile = "config1.ovpn" },
                    new { Country = "Germany", City = "Berlin", OvpnConfigFile = "config2.ovpn" }
                }
            };
            File.WriteAllText(tempInputFile, JsonSerializer.Serialize(sampleInput));

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(config => config.AddConsole());
                    services.AddSingleton<IFileHelper, FileHelper>();

                    services.AddSingleton<IBaseTestSpeedService, FakeBaseTestSpeedService>();
                    services.AddSingleton<IVpnSpeedTestService, FakeVpnSpeedTestService>();
                })
                .Build();

            ILogger logger = host.Services.GetRequiredService<ILogger<IntegrationTests>>();
            IFileHelper fileHelper = host.Services.GetRequiredService<IFileHelper>();
            IBaseTestSpeedService baseTestSpeedService = host.Services.GetRequiredService<IBaseTestSpeedService>();
            IVpnSpeedTestService vpnSpeedTestService = host.Services.GetRequiredService<IVpnSpeedTestService>();

            logger.LogInformation("Integration test started (simulated commands)");

            // Act - Run base speed test
            SpeedTestResult baseTestAverageResult = await baseTestSpeedService.RunSpeedTestsAsync(1);

            // Act - Read input file
            LocationsInput locations = fileHelper.ReadJsonFile(tempInputFile);

            // Act - Run VPN speed tests for each location
            var vpnStats = new List<VPNStats>();
            foreach (Location location in locations.Locations)
            {
                try
                {
                    // Use 1 test per location for speed.
                    var vpnStatsForLocation = await vpnSpeedTestService.RunVpnSpeedTestsAsync(location, 1);
                    logger.LogInformation("VPN speed test result for {Location}: {@Result}",
                        location, vpnStatsForLocation.VPNSpeed);
                    vpnStats.Add(vpnStatsForLocation);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error running VPN speed tests for location {Location}.", location);
                }
            }

            Output output = new Output
            {
                MachineName = Environment.MachineName,
                OS = Environment.OSVersion.VersionString,
                WithoutVPN = baseTestAverageResult,
                VPNStats = vpnStats,
            };

            // Act - Write results to output file
            await fileHelper.WriteResultsToFileAsync(tempOutputFile, output);

            // Assert 
            Assert.True(File.Exists(tempOutputFile), "Output file should exist.");
            string outputContent = File.ReadAllText(tempOutputFile);
            var outputDeserialized = JsonSerializer.Deserialize<Output>(outputContent);
            Assert.NotNull(outputDeserialized);
            Assert.Equal(Environment.MachineName, outputDeserialized!.MachineName);

            if (File.Exists(tempInputFile))
                File.Delete(tempInputFile);
            if (File.Exists(tempOutputFile))
                File.Delete(tempOutputFile);

            await host.StopAsync();
        }
    }
}
