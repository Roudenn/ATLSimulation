using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Systems;

// This part contains simplified API for interacting with gas mixture helpers
public abstract partial class SharedAtmosphericsSystem
{
    [PublicAPI]
    public GasMixture GetGridMixture(EntityUid grid)
    {
        var mixture = _atmosGridQuery.CompOrNull(grid)?.Mixture ?? GetSpaceMixture();
        GasManager.SetVolume(ref mixture, _subGrid.SubGridTileVolume);
        return mixture;
    }

    [PublicAPI]
    public GasMixture GetSpaceMixture()
    {
        return new GasMixture(GasManager.Count, _subGrid.SubGridTileVolume, PhysicalConstants.TCMB);
    }
}
