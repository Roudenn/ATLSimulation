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
        found = null;

        var (targetChunk, targetLocalIndex) = GetTileRelative(chunkPos, index, dir);

        if (!chunks.TryGetValue(targetChunk, out var chunk))
            return false;

        found = chunk[targetLocalIndex];
        return true;
    }

    public bool TryGetTemperatureTileRelative(
        Dictionary<Vector2i, TileTemperature[]> chunks,
        Vector2i chunkPos,
        int index,
        Vector2i dir,
        [NotNullWhen(true)] out TileTemperature? found)
    {
        found = null;

        var (targetChunk, targetLocalIndex) = GetTileRelative(chunkPos, index, dir);

        if (!chunks.TryGetValue(targetChunk, out var chunk))
            return false;

        found = chunk[targetLocalIndex];
        return true;
    }

    /// <summary>
    /// Finds relative position of a chunk and an index inside that chunk
    /// relative to some another index inside the given chunk.
    ///
    /// Basically a helper method to be able to move inside 1D array in 2 dimensions.
    /// </summary>
    /// <param name="chunkPos">Relative chunk position.</param>
    /// <param name="index">Index of a tile inside the chunk at <see cref="chunkPos"/>.</param>
    /// <param name="dir">The relative position of a target tile from <see cref="chunkPos"/> and <see cref="index"/>.</param>
    /// <returns></returns>
    public (Vector2i, int) GetTileRelative(Vector2i chunkPos, int index, Vector2i dir)
    {
        // Each subgrid chunk takes up at least 8x8 tiles.
        // Then, each tile can be divided further by at maximum 5 times,
        // making each tile contain 32x32 subtiles,
        // and each chunk will contain 256x256 subtiles in total.

        var x = index & (SubGridChunkSize - 1); // same as (index % ChunkSize)
        var y = index >> (SubGridDivisions + 3); // 3 because of the minimal chunk size being 8

        // Get the new index
        var tx = x + dir.X;
        var ty = y + dir.Y;

        // Get the target chunk that should contain the new index
        var targetChunkX = chunkPos.X + (tx >> (SubGridDivisions + 3));
        var targetChunkY = chunkPos.Y + (ty >> (SubGridDivisions + 3));

        // Calculate the new local index:
        // 1) Wrap the coordinate by using &
        // 2) Divide Y by ChunkSize (since Y represents current row)
        // 3) Add X and Y up to get the result
        var targetLocalIndex = (tx & (SubGridChunkSize - 1)) | ((ty & (SubGridChunkSize - 1)) << (SubGridDivisions + 3));

        return (new Vector2i(targetChunkX, targetChunkY), targetLocalIndex);
    }
}
