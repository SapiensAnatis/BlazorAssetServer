using System.Text.Json;
using AssetServer.Models;

namespace AssetServer.Services;

public class ConfigurationService
{
    private const string filepath = "config.json";

    public ConfigurationModel CurrentValue { get; private set; }

    public ConfigurationService()
    {
        CurrentValue = this.GetConfiguration();
    }

    public void SaveChanges(ConfigurationModel model)
    {
        File.WriteAllText(filepath, JsonSerializer.Serialize(model));

        CurrentValue = model;
    }

    private ConfigurationModel GetConfiguration()
    {
        if (!File.Exists(filepath))
            this.SaveChanges(new ConfigurationModel());

        ConfigurationModel result =
            JsonSerializer.Deserialize<ConfigurationModel>(File.ReadAllText(filepath))
            ?? throw new JsonException("Null deserialization result");

        return result;
    }
}
