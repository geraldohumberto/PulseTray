namespace PulseTray.Core;

public sealed class QueryDefinition
{
    public bool Enabled { get; set; }
    public string Name { get; set; } = "";
    public string Sql { get; set; } = "";
    public int AlertLimit { get; set; } = 15;
}
