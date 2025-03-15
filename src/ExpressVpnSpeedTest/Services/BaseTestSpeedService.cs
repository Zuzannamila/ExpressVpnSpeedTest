using ExpressVpnSpeedTest.Interfaces;
using ExpressVpnSpeedtestLibrary.Models;
using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using ExpressVpnSpeedTestLibrary.Models;
using Microsoft.Extensions.Logging;

namespace ExpressVpnSpeedTest.Services;

public class BaseTestSpeedService : IBaseTestSpeedService
{
    private readonly ILogger<BaseTestSpeedService> _logger;
    public BaseTestSpeedService(ILogger<BaseTestSpeedService> logger)
    {
        _logger = logger;
    }
    protected virtual Task<BufferedCommandResult> ExecuteSpeedtestAsync(CancellationToken cancellationToken)
    {
        return Cli.Wrap("speedtest")
            .WithArguments("--accept-license --accept-gdpr --format=json")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(cancellationToken);
    }

    public virtual async Task<SpeedTestResult> RunSpeedTestAsync()
    {
        // Cancel after a timeout of 90 seconds
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(90));

        try
        {
            var callresult = await ExecuteSpeedtestAsync(cts.Token);

            var exitCode = callresult.ExitCode;

            if (exitCode != 0)
            {
                _logger.LogError("Speedtest failed with exit code {ExitCode}.", exitCode);
                throw new InvalidOperationException("Speedtest Ookla failed with non-zero exit code.");
            }


            SpeedTestOoklaResult? rawResult;;
            try
            {
                rawResult = JsonSerializer.Deserialize<SpeedTestOoklaResult>(callresult.StandardOutput, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Error parsing JSON content from speedtest output: {Output}", callresult.StandardOutput);
                throw;
            }
            if(rawResult is null)
            {
                _logger.LogError("Speedtest Ookla output was null.");
                throw new InvalidOperationException("Speedtest Ookla output was null.");
            }

            var result = new SpeedTestResult
            {
                DownloadMbps = rawResult?.Download?.Bandwidth / 125000.0 ?? 0,
                UploadMbps = rawResult?.Upload?.Bandwidth / 125000.0 ?? 0,
                PingMs = rawResult?.Ping?.Latency ?? 0
            };
            _logger.LogInformation("Speedtest result: {@Result}", result);
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Speedtest timed out.");
            throw;
        }
    }

    public virtual async Task<SpeedTestResult> RunSpeedTestsAsync(int numberOfTests)
    {
        List<SpeedTestResult> singleTestsResults = new();

        for (int i = 0; i < numberOfTests; i++)
        {
            var result = await RunSpeedTestAsync();
            if (result != null)
            {
                singleTestsResults.Add(result);
            }
            else 
            {
                _logger.LogWarning("Speedtest failed. Skipping result.");
            }
        }
        // We can tolerate one failed test
        if (singleTestsResults.Count == 0 || singleTestsResults.Count < numberOfTests - 2)
        {
            throw new InvalidOperationException("Too many failed speedtests.");
        }

        var averageResult = new SpeedTestResult
        {
            DownloadMbps = Math.Round(singleTestsResults.Average(x => x.DownloadMbps)),
            UploadMbps = Math.Round(singleTestsResults.Average(x => x.UploadMbps)),
            PingMs = Math.Round(singleTestsResults.Average(x => x.PingMs))
        };

        _logger.LogInformation("Final avarage speedtest result: {@AverageResult}", averageResult);

        return averageResult;
    }
}