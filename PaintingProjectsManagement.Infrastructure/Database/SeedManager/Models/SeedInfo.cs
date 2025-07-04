﻿using Microsoft.EntityFrameworkCore;

namespace rbkApiModules.Commons.Relational;

public class SeedInfo<T> where T : DbContext
{
    public SeedInfo(Action<T, IServiceProvider> function, EnvironmentUsage environmentUsage)
    {
        Function = function;
        EnvironmentUsage = environmentUsage;
    }

    public Action<T, IServiceProvider> Function { get; }
    
    public EnvironmentUsage EnvironmentUsage { get; }
}

[Flags]
public enum EnvironmentUsage
{
    Production = 1,
    Development = 2,
    Staging = 3,
    Testing = 4
}