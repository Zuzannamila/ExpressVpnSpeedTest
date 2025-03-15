using ExpressVpnSpeedtestLibrary.Models;

namespace ExpressVpnSpeedTest.Interfaces;
public interface IVpnSpeedTestService
{
    Task<VPNStats> RunVpnSpeedTestsAsync(Location location, int numberOfTests);
}
