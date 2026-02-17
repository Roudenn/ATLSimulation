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

    public TileTemperature(float heatCapacity, float temperature)
    {
        var container = new HeatContainer(heatCapacity, temperature);
        Container = container;
        ArchivedContainer = container;
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
