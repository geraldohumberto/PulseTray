using System.Text.Json;
using PulseTray.Core;

namespace PulseTray.Configuration;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public string SettingsPath { get; }

    public SettingsStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        SettingsPath = Path.Combine(appData, "PulseTray", "settings.json");
    }

    public PulseTraySettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            var defaultSettings = CreateDefault();
            Save(defaultSettings);
            return defaultSettings;
        }

        var json = File.ReadAllText(SettingsPath);
        var settings = JsonSerializer.Deserialize<PulseTraySettings>(json, JsonOptions) ?? CreateDefault();
        EnsureFiveQuerySlots(settings);
        return settings;
    }

    public void Save(PulseTraySettings settings)
    {
        EnsureFiveQuerySlots(settings);
        var directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }

    private static PulseTraySettings CreateDefault()
    {
        var settings = new PulseTraySettings
        {
            Queries =
            [
                new QueryDefinition
                {
                    Enabled = true,
                    Name = "Novos",
                    Sql = """
                          SELECT COUNT(*) AS total_novos
                          FROM main_score
                          WHERE empresa = 'ATN'
                            AND LOWER(status_atual) = 'novo'
                            AND (
                                  DATE(data_recebimento) = CURDATE()
                                  OR DATE(data_update) = CURDATE()
                                );
                          """,
                    AlertLimit = 15
                }
            ]
        };

        EnsureFiveQuerySlots(settings);
        return settings;
    }

    private static void EnsureFiveQuerySlots(PulseTraySettings settings)
    {
        settings.RefreshMinutes = Math.Max(1, settings.RefreshMinutes);
        settings.Queries ??= [];

        while (settings.Queries.Count < 5)
        {
            settings.Queries.Add(new QueryDefinition
            {
                Enabled = false,
                Name = $"Query {settings.Queries.Count + 1}",
                AlertLimit = 15
            });
        }

        if (settings.Queries.Count > 5)
        {
            settings.Queries = settings.Queries.Take(5).ToList();
        }
    }
}
