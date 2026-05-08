namespace PulseTray.Core;

public sealed class PulseTraySettings
{
    public DatabaseSettings Database { get; set; } = new();
    public int RefreshMinutes { get; set; } = 10;
    public List<QueryDefinition> Queries { get; set; } = [];

    public IEnumerable<QueryDefinition> EnabledQueries()
    {
        return Queries.Where(query => query.Enabled && !string.IsNullOrWhiteSpace(query.Sql));
    }
}
