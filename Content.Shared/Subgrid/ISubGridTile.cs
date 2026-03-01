namespace Content.Shared.Subgrid;

public interface ISubGridTile
{
    /// <summary>
    /// This is basically a hack that allows to distinct default
    /// uninitialized values in an array of structs from initialized tiles.
    /// False for "empty" subtiles and true for subtiles that can be generally used.
    /// </summary>
    bool Initialized { get; set; }

    int CurrentTick { get; set; }

    int LastTick { get; set; }
}
