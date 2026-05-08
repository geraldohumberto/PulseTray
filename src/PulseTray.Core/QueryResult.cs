namespace PulseTray.Core;

public sealed class QueryResult
{
    public QueryResult(QueryDefinition query, long value, DateTimeOffset checkedAt)
    {
        Query = query;
        Value = value;
        CheckedAt = checkedAt;
    }

    public QueryDefinition Query { get; }
    public long Value { get; }
    public DateTimeOffset CheckedAt { get; }
    public bool IsAlert => Value > Query.AlertLimit;
}
