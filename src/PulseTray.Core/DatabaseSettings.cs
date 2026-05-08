namespace PulseTray.Core;

public sealed class DatabaseSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string Database { get; set; } = "";
}
