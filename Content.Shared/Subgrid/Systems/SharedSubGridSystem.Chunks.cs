using System.Diagnostics.CodeAnalysis;
using Content.Shared.Atmospherics;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    public void ResolveAtmosphereChunkMap(Entity<SubGridComponent> grid, ref Dictionary<Vector2i, TileAtmosphere[]> map)
    {
        foreach (var (chunkPos, chunk) in grid.Comp.ChunkEntities)
        {
            var chunkComp = _chunkQuery.Comp(chunk);
            map.Add(chunkPos, chunkComp.AtmosphereMap);
        }
    }
    
    public void ResolveTemperatureChunkMap(Entity<SubGridComponent> grid, ref Dictionary<Vector2i, TileTemperature[]> map)
    {
        foreach (var (chunkPos, chunk) in grid.Comp.ChunkEntities)
        {
            var chunkComp = _chunkQuery.Comp(chunk);
            map.Add(chunkPos, chunkComp.TemperatureMap);
        }
    }
    
    public bool TryGetAtmosphereTileRelative(
        Dictionary<Vector2i, TileAtmosphere[]> chunks,
        Vector2i chunkPos,
        int index,
        Vector2i dir,
        [NotNullWhen(true)] out TileAtmosphere? found)
    {
        // TODO TESTS: unit test and benchmark this
        found = null;
        
        var x = index & 7;
        var y = index >> 3;

        var tx = x + dir.X;
        var ty = y + dir.Y;
        
        var targetChunkX = chunkPos.X + (tx >> 3);
        var targetChunkY = chunkPos.Y + (ty >> 3);
        
        var targetLocalIndex = (tx & 7) | ((ty & 7) << 3);

        if (!chunks.TryGetValue((targetChunkX, targetChunkY), out var chunk))
            return false;
        
        found = chunk[targetLocalIndex];
        return true;
    }
    
    public bool TryGetTemperatureTileRelative(Dictionary<Vector2i, TileTemperature[]> chunks, int index, Vector2i chunkPos, Vector2i dir, [NotNullWhen(true)] out TileTemperature? found)
    {
        // TODO TESTS: unit test and benchmark this
        found = null;
        
        var x = index & 7;
        var y = index >> 3;

        var tx = x + dir.X;
        var ty = y + dir.Y;
        
        var targetChunkX = chunkPos.X + (tx >> 3);
        var targetChunkY = chunkPos.Y + (ty >> 3);
        
        var targetLocalIndex = (tx & 7) | ((ty & 7) << 3);

        if (!chunks.TryGetValue((targetChunkX, targetChunkY), out var chunk))
            return false;
        
        found = chunk[targetLocalIndex];
        return true;
    }
}
