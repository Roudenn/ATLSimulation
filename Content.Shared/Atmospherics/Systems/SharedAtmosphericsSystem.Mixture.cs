using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Atmospherics.Prototypes;
using Content.Shared.Constants;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Systems;

// This part contains simplified API for interacting with gas mixture helpers
public abstract partial class SharedAtmosphericsSystem
{
    /// <summary>
    /// Creates a gas mixture based on a prototype with explicitly specified volume.
    /// </summary>
    [PublicAPI]
    public IGasMixture ResolveMixture(ProtoId<GasMixturePrototype> mix, float volume)
    {
        var proto = _protoMan.Index(mix);
        return proto.Definition.CreateMixture(GasManager, _protoMan, volume);
    }

    [PublicAPI]
    public GasMixture GetGridMixture(EntityUid grid)
    {
        var mixture = GetSpaceTileMixture();
        if (_atmosGridQuery.TryComp(grid, out var gridComp))
            mixture = (GasMixture) _protoMan.Index(gridComp.Mixture)
                .Definition.CreateMixture(
                    GasManager,
                    _protoMan,
                    _subGrid.SubGridTileVolume);
        return mixture;
    }

    [PublicAPI]
    public GasMixture GetSpaceTileMixture()
    {
        return new GasMixture(GasManager.ArraySize, _subGrid.SubGridTileVolume, PhysicalConstants.TCMB);
    }
}
