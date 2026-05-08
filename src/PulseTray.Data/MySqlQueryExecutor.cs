using MySqlConnector;
using PulseTray.Core;

namespace PulseTray.Data;

public sealed class MySqlQueryExecutor : IQueryExecutor
{
    public async Task TestConnectionAsync(DatabaseSettings settings, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(BuildConnectionString(settings));
        await connection.OpenAsync(cancellationToken);
    }

    public async Task<QueryResult> ExecuteCountAsync(DatabaseSettings settings, QueryDefinition query, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(BuildConnectionString(settings));
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = query.Sql;
        command.CommandTimeout = 30;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        var value = Convert.ToInt64(result ?? 0);
        return new QueryResult(query, value, DateTimeOffset.Now);
    }

    private static string BuildConnectionString(DatabaseSettings settings)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = settings.Host,
            Port = (uint)Math.Clamp(settings.Port, 1, 65535),
            UserID = settings.User,
            Password = settings.Password,
            Database = settings.Database,
            SslMode = MySqlSslMode.Preferred,
            ConnectionTimeout = 10,
            DefaultCommandTimeout = 30
        };

        return builder.ConnectionString;
    }
}
