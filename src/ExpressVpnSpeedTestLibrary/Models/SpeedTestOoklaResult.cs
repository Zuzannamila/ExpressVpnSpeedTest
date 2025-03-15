namespace ExpressVpnSpeedTestLibrary.Models;
public class SpeedTestOoklaResult
{
    public required PingInfo Ping { get; set; }
    public required BandwidthInfo Download { get; set; }
    public required BandwidthInfo Upload { get; set; }
}

public class PingInfo
{
    public double Latency { get; set; }
}

public class BandwidthInfo
{
    public double Bandwidth { get; set; }
}
