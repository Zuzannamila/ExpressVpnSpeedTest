using System.Text.Json;
using ExpressVpnSpeedtestLibrary.Models;
using Microsoft.Extensions.Logging;

namespace ExpressVpnSpeedTest.Utils;

public interface IFileHelper
{
    LocationsInput ReadJsonFile(string path);
    Task WriteResultsToFileAsync(string path, Output result);
}

public class FileHelper : IFileHelper
{
    private readonly ILogger<FileHelper> _logger;
    public FileHelper(ILogger<FileHelper> logger)
    {
        _logger = logger;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    #region Rading JSON file
    public LocationsInput ReadJsonFile(string path)
    {
        if (!File.Exists(path))
        {
            _logger.LogError("Input file not found: {Path}", path);
            throw new FileNotFoundException("Input file not found: {path}", path);
        }
        try
        {
            var rawJson = File.ReadAllText(path);
            var locations = JsonSerializer.Deserialize<LocationsInput>(rawJson, _jsonOptions);
            if (locations is null || locations.Locations is null || locations.Locations.Count == 0)
            {
                _logger.LogError("Deserialized locations are null or empty. Path: {Path}", path);
                throw new InvalidDataException($"Could not deserialize locations from file: {path}");
            }

            _logger.LogInformation("Succesfully read {Count} locations from file: {Path}", locations.Locations.Count, path);
            return locations;
        }
        catch(JsonException jex)
        {
            _logger.LogError(jex, "Error parsing JSON content from {Path}", path);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reading file: {Path}", path);
            throw;
        }
    }
    #endregion

    #region Writing JSON file
    public async Task WriteResultsToFileAsync(string path, Output results)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(results, _jsonOptions);
            await File.WriteAllTextAsync(path, jsonContent);
            _logger.LogInformation("Successfully wrote results to file: {FilePath}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing results to file: {FilePath}", path);
            throw;
        }
    }
    #endregion
}