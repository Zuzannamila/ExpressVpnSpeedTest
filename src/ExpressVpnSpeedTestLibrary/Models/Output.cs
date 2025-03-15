namespace ExpressVpnSpeedtestLibrary.Models;

public class Output 
{
    public required string MachineName { get; set; }
    public required string OS { get; set; }
    public required SpeedTestResult WithoutVPN { get; set; }
    public required List<VPNStats> VPNStats { get; set; }
}

public class VPNStats
{
    public required string LocationName{ get; set; }
    public required string TimeToConnect { get; set; }
    public required SpeedTestResult VPNSpeed { get; set; }
}
