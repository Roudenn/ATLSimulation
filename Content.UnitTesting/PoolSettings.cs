namespace Content.UnitTesting;

/// <inheritdoc/>
public sealed class PoolSettings : PairSettings
{
    public override bool Connected { get; init; }

    /// <summary>
    /// If true, this enables the creation of admin logs during the test.
    /// </summary>
    public bool AdminLogsEnabled { get; init; }

    /// <summary>
    /// Set this to true to skip loading the content files.
    /// Note: This setting won't work with a client.
    /// </summary>
    public bool NoLoadContent { get; init; }

    /// <summary>
    /// Set this to the path of a map to have the given server/client pair load the map.
    /// </summary>
    public string Map { get; init; } = PoolManager.TestMap;

    public override bool CanFastRecycle(PairSettings nextSettings)
    {
        if (!base.CanFastRecycle(nextSettings))
            return false;

        if (nextSettings is not PoolSettings next)
            return false;

        // Check that certain settings match.
        return Map == next.Map;
    }
}
