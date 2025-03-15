namespace ExpressVpnSpeedtestLibrary.Models;

public class SpeedTestResult
{
    public double DownloadMbps { get; set; }
    public double UploadMbps { get; set; }
    public double PingMs { get; set; }

    public override string ToString()
    {
        return $"Download speed: {DownloadMbps} Mbps, Upload speed: {UploadMbps} Mbps, Ping: {PingMs} Mbps";
    }
}

