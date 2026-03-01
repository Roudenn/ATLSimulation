using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Atmospherics.Systems;

namespace Content.Client.Atmospherics;

public sealed class AtmosphericsSystem : SharedAtmosphericsSystem
{
    public GasMixEntry GenerateGaxMixEntry(GasMixture m)
    {
        var gases = new GasEntry[GasManager.Count];
        for (var index = 0; index < m.Moles.Length; index++)
        {
            var gasAmount = m.Moles[index];
            var gasProto = GasManager[index];
            gases[index] = new GasEntry(gasProto.Name, gasAmount, gasProto.Color);
        }

        return new GasMixEntry(m.Volume, m.Pressure, m.HeatContainer, gases);
    }
}
