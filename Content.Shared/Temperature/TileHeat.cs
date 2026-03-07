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
    public HeatContainer Container;

    /// <summary>
    /// The main container of this temperature tile.
    /// </summary>
    [DataField]
    public HeatContainer ArchivedContainer;

    [ViewVariables]
    public int CurrentTick { get; set; }

    [ViewVariables]
    public int LastTick { get; set; }

    /// <summary>
    /// This is basically a hack that allows to distinct default
    /// uninitialized values in an array of structs from initialized tiles.
    /// False for "empty" subtiles and true for subtiles that can be generally used.
    /// </summary>
    [ViewVariables]
    public bool Initialized { get; set; } = true;

    public TileHeat(float heatCapacity, float temperature, float thermalConductance, bool immutable = false)
    {
        Container = new HeatContainer(heatCapacity, temperature, thermalConductance, immutable);
        ArchivedContainer = new HeatContainer(heatCapacity, temperature, thermalConductance, immutable);
    }

    public TileHeat(HeatContainer container)
    {
        Container = container;
        ArchivedContainer = container;
    }

    private TileHeat(TileHeat c)
    {
        Container = c.Container;
        ArchivedContainer = c.ArchivedContainer;
    }

    public TileHeat Clone()
    {
        return new TileHeat(this);
    }
}
