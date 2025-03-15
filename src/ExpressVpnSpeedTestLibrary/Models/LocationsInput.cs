namespace ExpressVpnSpeedtestLibrary.Models;
public class LocationsInput
{
    public required List<Location> Locations { get; set; } = new();
}

public class Location
{
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string OvpnConfigFile { get; set; }
    public override string ToString()
    {
        return $"{City}, {Country}";
    }
}