using System.Threading;
using System.Threading.Tasks;
using CliWrap.Buffered;
using ExpressVpnSpeedTest.Services;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.Logging;
using ExpressVpnSpeedTest.Interfaces;


namespace ExpressVpnSpeedTest.IntegrationTests
{
    public class FakeBaseTestSpeedService : BaseTestSpeedService
    {
        public FakeBaseTestSpeedService(ILogger<BaseTestSpeedService> logger)
            : base(logger) { }

        protected override Task<BufferedCommandResult> ExecuteSpeedtestAsync(CancellationToken cancellationToken)
        {
            string fakeJsonOutput = "{\"download\":{\"bandwidth\":12500000},\"upload\":{\"bandwidth\":6250000},\"ping\":{\"latency\":15}}";
            
            var startTime = DateTimeOffset.Now;
            var exitTime = startTime.AddSeconds(1);
            
            var fakeResult = new BufferedCommandResult(0, startTime, exitTime, fakeJsonOutput, string.Empty);
            return Task.FromResult(fakeResult);
        }
    }

    public class FakeVpnSpeedTestService : VpnSpeedTestService
    {
        public FakeVpnSpeedTestService(IBaseTestSpeedService baseTestSpeedService, ILogger<VpnSpeedTestService> logger)
            : base(baseTestSpeedService, logger) { }

        protected override Task<double> ConnectVpnAsync(string ovpnConfigFile)
        {
            return Task.FromResult(2.0);
        }

        protected override Task DisconnectVpnAsync()
        {
            return Task.CompletedTask;
        }
    }
}
