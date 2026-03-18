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
    public GasMixture ResolveMixture(ProtoId<GasMixturePrototype> mix, float volume)
    {
        var proto = _protoMan.Index(mix);
        var array = new float[GasManager.ArraySize];
        foreach (var (id, value) in proto.Moles)
        {
            if (!_protoMan.Resolve<GasPrototype>(id, out var gasProto))
                continue;

            array[gasProto.GasId] = value;
        }

        return new GasMixture(array, volume, proto.Temperature);
    }

    [PublicAPI]
    public GasMixture GetGridMixture(EntityUid grid)
    {
        var mixture = _atmosGridQuery.CompOrNull(grid)?.Mixture ?? GetSpaceTileMixture();
        GasManager.SetVolume(ref mixture, _subGrid.SubGridTileVolume);
        return mixture;
    }

    [PublicAPI]
    public GasMixture GetSpaceTileMixture()
    {
        return new GasMixture(GasManager.ArraySize, _subGrid.SubGridTileVolume, PhysicalConstants.TCMB);
    }
}
