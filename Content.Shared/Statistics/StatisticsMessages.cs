using Content.Shared.Maps;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Statistics;

[Serializable, NetSerializable]
public sealed class StatisticsMessage(SimulationStats stats) : EntityEventArgs
{
    public SimulationStats Stats = stats;
}

[Serializable, NetSerializable]
public struct SimulationStats(
    ProtoId<SimulationMapPrototype> currentMap,
    int tileCount,
    int chunkCount) : IEquatable<SimulationStats>
{
    public ProtoId<SimulationMapPrototype> CurrentMap = currentMap;
    public int TileCount = tileCount;
    public int ChunkCount = chunkCount;

    public bool Equals(SimulationStats? other)
    {
        return other != null
               && CurrentMap == other.Value.CurrentMap
               && TileCount == other.Value.TileCount
               && ChunkCount == other.Value.ChunkCount;
    }

    public bool Equals(SimulationStats other)
    {
        return CurrentMap.Equals(other.CurrentMap) && TileCount == other.TileCount && ChunkCount == other.ChunkCount;
    }

    public override bool Equals(object? obj)
    {
        return obj is SimulationStats other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CurrentMap, TileCount, ChunkCount);
    }

    public static bool operator ==(SimulationStats left, SimulationStats right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SimulationStats left, SimulationStats right)
    {
        return !(left == right);
    }
}
