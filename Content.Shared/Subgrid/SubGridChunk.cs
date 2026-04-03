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

    public SubGridChunk(VelocityGasMixture[] mixtures, ConductiveHeatContainer[] containers)
    {
        TemperatureMap = new TileHeat[containers.Length];
        for (int i = 0; i < containers.Length; i++)
        {
            TemperatureMap[i] = new TileHeat(containers[i]);
        }
        AtmosphereMap = new TileAtmos[mixtures.Length];
        for (int i = 0; i < mixtures.Length; i++)
        {
            AtmosphereMap[i] = new TileAtmos(mixtures[i]);
        }
    }
}
