namespace Content.Shared.Subgrid;

public interface ISubGridTile
{
    /// <summary>
    /// A marker bool that indicates whenever this tile is available for processing,
    /// or if it's just a default value.
    /// Always check that this is true before interacting with the tile.
    /// </summary>
    /// <remarks>
    /// This is only false if the current subtile is considered to be "empty".
    /// </remarks>
    bool Initialized { get; set; }

    int CurrentTick { get; set; }

    int LastTick { get; set; }
}
