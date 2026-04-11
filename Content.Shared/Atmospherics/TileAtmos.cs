using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Subgrid;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmospherics;

[DataDefinition, Serializable, NetSerializable]
public partial struct TileAtmos : IRobustCloneable<TileAtmos>, ISubGridTile
{
    /// <summary>
    /// The gas mixture that is contained on this subtile.
    /// </summary>
    /// <remarks>
    /// This is write-only data when calculating the new atmos state.
    /// If you need to get safe information about the mixture, check <see cref="Mixture"/>
    /// </remarks>
    [DataField]
    public GasMixture CachedMixture;

    /// <summary>
    /// Contains the state of tile's gas mixture on the previous atmos tick.
    /// When the tile is first created, it's the same as the basic <see cref="CachedMixture"/>.
    /// </summary>
    [DataField]
    public GasMixture Mixture;

    /// <summary>
    /// If true, this tile shares the mixture of air with the parent map.
    /// It also makes the GasMixture immutable.
    /// </summary>
    [DataField]
    public bool MapAtmosphere;

    [ViewVariables]
    public bool Initialized { get; set; } = true;

    [ViewVariables]
    public int CurrentTick { get; set; }

    [ViewVariables]
    public int LastTick { get; set; }

    public TileAtmos(GasMixture cachedMixture)
    {
        CachedMixture = cachedMixture;
        Mixture = cachedMixture;
        MapAtmosphere = cachedMixture.Immutable;
    }

    public TileAtmos(TileAtmos c)
    {
        CachedMixture = c.CachedMixture;
        Mixture = c.Mixture;
        MapAtmosphere = c.MapAtmosphere;
    }

    public TileAtmos Clone()
    {
        return new TileAtmos(this);
    }
}
