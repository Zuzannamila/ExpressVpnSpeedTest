using System.Text.Json;
using ExpressVpnSpeedTest.Interfaces;
using ExpressVpnSpeedTest.Services;
using ExpressVpnSpeedTest.Utils;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging(config =>
        {
            config.AddConsole(); 
        });
        services.AddSingleton<IFileHelper, FileHelper>();
        services.AddSingleton<IBaseTestSpeedService, BaseTestSpeedService>();
        services.AddSingleton<IVpnSpeedTestService, VpnSpeedTestService>();
    })
    .Build();

ILogger logger = host.Services.GetRequiredService<ILogger<Program>>();
IFileHelper fileHelper = host.Services.GetRequiredService<IFileHelper>();
IBaseTestSpeedService baseTestSpeedService = host.Services.GetRequiredService<IBaseTestSpeedService>();
IVpnSpeedTestService vpnSpeedTestService = host.Services.GetRequiredService<IVpnSpeedTestService>();

logger.LogInformation("Test started");

LocationsInput locations = null!;
SpeedTestResult baseTestAverageResult = null!;
List<VPNStats> vpnStats = new();

#region Read input file
try
{
    locations = fileHelper.ReadJsonFile("input.json");
}
catch(FileNotFoundException ex)
{
    logger.LogError(ex, "File not found. Exiting program.");
    Environment.Exit(1);
}
catch(InvalidDataException ex)
{
    logger.LogError(ex, "Invalid data. Exiting program.");
    Environment.Exit(1);
}
catch(JsonException jex)
{
    logger.LogError(jex, "Error parsing JSON content. Exiting program.");
    Environment.Exit(1);
}
catch(Exception ex)
{
    logger.LogError(ex, "Unexpected error. Exiting program.");
    Environment.Exit(1);
}
#endregion

#region Base speed test
try
{
    baseTestAverageResult = await baseTestSpeedService.RunSpeedTestsAsync(5);
}
catch(InvalidOperationException ex)
{
    logger.LogError(ex, "Speedtest failed. Exiting program.");
    Environment.Exit(1);
}
catch(Exception ex)
{
    logger.LogError(ex, "Unexpected error. Exiting program.");
    Environment.Exit(1);
}
#endregion

#region VPN speed test
foreach(Location location in locations.Locations)
{
    try
    {
        var vpnStatsForLocation = await vpnSpeedTestService.RunVpnSpeedTestsAsync(location, 5);
        logger.LogInformation("Average speed test result for {Location}: {@Result}", location, vpnStatsForLocation.VPNSpeed);
        vpnStats.Add(vpnStatsForLocation);
    }
    catch(Exception ex)
    {
        logger.LogError(ex, "Error running VPN speed tests for location {Location}. Skipping location.", location);
    }
}
#endregion

Output output = new Output
{
    MachineName = Environment.MachineName,
    OS = Environment.OSVersion.VersionString,
    WithoutVPN = baseTestAverageResult,
    VPNStats = vpnStats,
};

#region Write results to file
try 
{
    await fileHelper.WriteResultsToFileAsync("/app/output/output.json", output);
}
catch(Exception ex)
{
    logger.LogError(ex, "Error writing results to file. Exiting program.");
    Environment.Exit(1);
}
#endregion

await host.StopAsync();