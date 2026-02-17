using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Subgrid;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmospherics;

[DataDefinition, Serializable, NetSerializable]
public partial struct TileAtmosphere : IRobustCloneable<TileAtmosphere>, ISubGridTile
{
    /// <summary>
    /// The gas mixture that is contained on this subtile.
    /// </summary>
    /// <remarks>
    /// This is write-only data when calculating the new atmos state.
    /// If you need to get safe information about the mixture, check <see cref="ArchivedMixture"/>
    /// </remarks>
    [DataField]
    public GasMixture Mixture;

    /// <summary>
    /// Contains the state of tile's gas mixture on the previous atmos tick.
    /// When the tile is first created, it's the same as the basic <see cref="Mixture"/>.
    /// </summary>
    [DataField]
    public GasMixture ArchivedMixture;

    /// <summary>
    /// If true, this tile shares the mixture of air with the parent map.
    /// It also makes the GasMixture immutable.
    /// </summary>
    [DataField]
    public bool MapAtmosphere;

    public TileAtmosphere(GasMixture mixture)
    {
        Mixture = mixture;
        ArchivedMixture = mixture;
    }

    public TileAtmosphere(GasMixture mixture, bool mapAtmosphere)
    {
        Mixture = mixture;
        ArchivedMixture = mixture;
        MapAtmosphere = mapAtmosphere;
    }

    private TileAtmosphere(TileAtmosphere c)
    {
        Mixture = c.Mixture;
        ArchivedMixture = c.ArchivedMixture;
        MapAtmosphere = c.MapAtmosphere;
    }

    public TileAtmosphere Clone()
    {
        return new TileAtmosphere(this);
    }
}
