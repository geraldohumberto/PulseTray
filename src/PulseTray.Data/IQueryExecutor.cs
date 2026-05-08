using PulseTray.Core;

namespace PulseTray.Data;

public interface IQueryExecutor
{
    Task TestConnectionAsync(DatabaseSettings settings, CancellationToken cancellationToken = default);
    Task<QueryResult> ExecuteCountAsync(DatabaseSettings settings, QueryDefinition query, CancellationToken cancellationToken = default);
}
