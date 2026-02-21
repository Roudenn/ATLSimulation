using Content.Shared.Subgrid;
using Content.Shared.Temperature.HeatContainers;
using Robust.Shared.Serialization;

namespace Content.Shared.Temperature;

[DataDefinition, Serializable, NetSerializable]
public partial struct TileTemperature : IRobustCloneable<TileTemperature>, ISubGridTile
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

    [DataField]
    public bool Initialized { get; set; }

    [ViewVariables]
    public int CurrentTick { get; set; }

    [ViewVariables]
    public int LastTick { get; set; }

    public TileTemperature(float heatCapacity, float temperature, bool initialized = false)
    {
        Container = new HeatContainer(heatCapacity, temperature);
        ArchivedContainer = new HeatContainer(heatCapacity, temperature);
        Initialized = initialized;
    }

    public TileTemperature(HeatContainer container)
    {
        Container = container;
        ArchivedContainer = container;
    }

    private TileTemperature(TileTemperature c)
    {
        Container = c.Container;
        ArchivedContainer = c.ArchivedContainer;
    }

    public TileTemperature Clone()
    {
        return new TileTemperature(this);
    }
}
