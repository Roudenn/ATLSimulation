using Content.Shared.Subgrid;
using Content.Shared.Temperature.HeatContainers;
using Robust.Shared.Serialization;

namespace Content.Shared.Temperature;

[DataDefinition, Serializable, NetSerializable]
public partial struct TileHeat : IRobustCloneable<TileHeat>, ISubGridTile
{
    /// <summary>
    /// The main container of this temperature tile.
    /// </summary>
    [DataField]
    public ConductiveHeatContainer CachedContainer;

    /// <summary>
    /// The main container of this temperature tile.
    /// </summary>
    [DataField]
    public ConductiveHeatContainer Container;

    /// <summary>
    /// This is basically a hack that allows to distinct default
    /// uninitialized values in an array of structs from initialized tiles.
    /// False for "empty" subtiles and true for subtiles that can be generally used.
    /// </summary>
    [ViewVariables]
    public bool Initialized { get; set; } = true;

    public TileHeat(float heatCapacity, float temperature, float thermalConductance, bool immutable = false)
    {
        CachedContainer = new ConductiveHeatContainer(heatCapacity, temperature, thermalConductance, immutable);
        Container = new ConductiveHeatContainer(heatCapacity, temperature, thermalConductance, immutable);
    }

    public TileHeat(ConductiveHeatContainer cachedContainer)
    {
        CachedContainer = cachedContainer;
        Container = cachedContainer;
    }

    public TileHeat(TileHeat c)
    {
        CachedContainer = c.CachedContainer;
        Container = c.Container;
    }

    public TileHeat Clone()
    {
        return new TileHeat(this);
    }
}
