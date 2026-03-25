using Content.Shared.Atmospherics;
using Content.Shared.Temperature;
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

    public SubGridChunk(int chunkArea)
    {
        TemperatureMap = new TileHeat[chunkArea];
        AtmosphereMap = new TileAtmos[chunkArea];
    }
}
