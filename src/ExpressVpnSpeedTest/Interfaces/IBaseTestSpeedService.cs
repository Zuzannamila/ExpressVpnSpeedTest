using ExpressVpnSpeedtestLibrary.Models;

namespace ExpressVpnSpeedTest.Interfaces;
public interface IBaseTestSpeedService
{
    Task<SpeedTestResult> RunSpeedTestAsync();
    Task<SpeedTestResult> RunSpeedTestsAsync(int numberOfTests);
}