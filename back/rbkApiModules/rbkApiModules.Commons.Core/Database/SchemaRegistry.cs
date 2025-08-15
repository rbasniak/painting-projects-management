using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace rbkApiModules.Commons.Relational;

public static class SchemaRegistry
{
    private static readonly ConcurrentDictionary<Type, (string Schema, string Table)> _relationalMappings = new();

    public static void AddRelationalMapping<TEntity>(string schema, string table)
    {
        _relationalMappings[typeof(TEntity)] = (schema, table);
    }

    public static (string Schema, string Table)? GetRelationalMapping<TEntity>()
    {
        return _relationalMappings.TryGetValue(typeof(TEntity), out var mapping) ? mapping : null;
    }
}
