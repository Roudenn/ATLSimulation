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
    
    private TileTemperature(TileTemperature c)
    {
        Container = c.Container;
    }
    
    public TileTemperature Clone()
    {
        return new TileTemperature(this);
    }
}
