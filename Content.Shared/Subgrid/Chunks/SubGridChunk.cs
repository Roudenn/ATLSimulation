using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using Content.Shared.Temperature;
using Content.Shared.Temperature.HeatContainers;

namespace Content.Shared.Subgrid.Chunks;

public sealed class SubGridChunk
{
    [ViewVariables]
    public readonly int ChunkSize;

    public int ChunkArea => ChunkSize * ChunkSize;

    public int TileSize => ChunkSize / SystemConstants.PvsChunkSize;

    [ViewVariables]
    public TileHeat[] TemperatureMap;

    [ViewVariables]
    public TileAtmos[] AtmosphereMap;

    [ViewVariables]
    public int CurrentTick;

    [ViewVariables]
    public int LastTick;

    public SubGridChunk(int chunkSize)
    {
        ChunkSize = chunkSize;

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

    /// <summary>
    /// Sets the state of all arrays to this subgrid chunk, also clearing all previous values.
    /// </summary>
    /// <param name="mixtures"></param>
    /// <param name="containers"></param>
    public void ApplyState(GasMixture[] mixtures, ConductiveHeatContainer[] containers)
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
