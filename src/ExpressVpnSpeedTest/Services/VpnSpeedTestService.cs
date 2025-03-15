using System.Diagnostics;
using CliWrap;
using ExpressVpnSpeedTest.Interfaces;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.Logging;

namespace ExpressVpnSpeedTest.Services;

public class VpnSpeedTestService : IVpnSpeedTestService
{
    private readonly IBaseTestSpeedService _baseTestSpeedService;
    private readonly ILogger<VpnSpeedTestService> _logger;

    public VpnSpeedTestService(IBaseTestSpeedService baseTestSpeedService, ILogger<VpnSpeedTestService> logger)
    {
        _baseTestSpeedService = baseTestSpeedService;
        _logger = logger;
    }

    protected virtual async Task<double> ConnectVpnAsync(string ovpnConfigFile)
    {
        var stopWatch = Stopwatch.StartNew();

        await Cli.Wrap("openvpn")
            .WithArguments($"--config /vpn/{ovpnConfigFile} --auth-user-pass /vpn/expressvpn.auth --daemon")
            .ExecuteAsync();
        await Task.Delay(TimeSpan.FromSeconds(10));
        _logger.LogInformation("Connected to VPN using config file {VpnConfigFile}, after assummed 10 seconds", ovpnConfigFile);

        return stopWatch.Elapsed.TotalSeconds;
    }

    protected virtual async Task DisconnectVpnAsync()
    {
        await Cli.Wrap("pkill").WithArguments("openvpn").ExecuteAsync();
        _logger.LogInformation("VPN disconnected.");
        await Task.Delay(TimeSpan.FromSeconds(5)); 
    }

    public async Task<VPNStats> RunVpnSpeedTestsAsync(Location location, int numberOfTests)
    {
        try
        {
            double timeToConnect = await ConnectVpnAsync(location.OvpnConfigFile);
            SpeedTestResult speedTestResult = await _baseTestSpeedService.RunSpeedTestsAsync(numberOfTests);
            VPNStats vpnStats = new VPNStats
            {
                LocationName = location.ToString(),
                TimeToConnect = $"{timeToConnect.ToString("F2")} seconds",
                VPNSpeed = speedTestResult,

            };
            return vpnStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running VPN speed tests with config {Config}", location.OvpnConfigFile);
            throw;
        }
        finally
        {
            await DisconnectVpnAsync();
        }
    }
      
}