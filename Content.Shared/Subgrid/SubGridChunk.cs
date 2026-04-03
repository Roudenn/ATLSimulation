using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Temperature;
using Content.Shared.Temperature.HeatContainers;
using Robust.Shared.Serialization;

namespace Content.Shared.Subgrid;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class SubGridChunk
{
    [DataField]
    public TileHeat[] TemperatureMap;

    [DataField]
    public TileAtmos[] AtmosphereMap;

    [ViewVariables]
    public int CurrentTick;

    [ViewVariables]
    public int LastTick;

    public SubGridChunk(int chunkSize)
    {
        TemperatureMap = new TileHeat[chunkSize * chunkSize];
        AtmosphereMap = new TileAtmos[chunkSize * chunkSize];
    }

    public SubGridChunk(SubGridChunk other)
    {
        TemperatureMap = new TileHeat[other.TemperatureMap.Length];
        AtmosphereMap = new TileAtmos[other.AtmosphereMap.Length];
        other.TemperatureMap.AsSpan().CopyTo(TemperatureMap);
        other.AtmosphereMap.AsSpan().CopyTo(AtmosphereMap);
    }

    public void ApplyState(VelocityGasMixture[] mixtures, ConductiveHeatContainer[] containers)
    {
        var tempSpan = TemperatureMap.AsSpan();
        tempSpan.Clear();
        tempSpan.CopyTo(TemperatureMap);

        var atmosSpan = AtmosphereMap.AsSpan();
        atmosSpan.Clear();
        atmosSpan.CopyTo(AtmosphereMap);

        for (int i = 0; i < containers.Length; i++)
        {
            if (!containers[i].Initialized)
                continue;

            TemperatureMap[i] = new TileHeat(containers[i]);
        }

        for (int i = 0; i < mixtures.Length; i++)
        {
            if (!mixtures[i].Initialized)
                continue;

            AtmosphereMap[i] = new TileAtmos(mixtures[i]);
        }
    }
}
