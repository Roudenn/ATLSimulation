using System.Diagnostics.CodeAnalysis;
using Content.Shared.Atmospherics;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature;
using JetBrains.Annotations;

namespace Content.Shared.Subgrid.Systems;

// General API to interact with subgrids.
public abstract partial class SharedSubGridSystem
{
    /// <summary>
    /// Gets all atmosphere chunks on a grid and adds them to a given dictionary.
    /// </summary>
    /// <remarks>
    /// Remember to clear the dictionary before passing any information into it!
    /// </remarks>
    /// <param name="grid">A grid to find the chunks on.</param>
    /// <param name="map">A cache dictionary to add the chunk data.</param>
    [PublicAPI]
    public void ResolveAtmosphereChunkMap(Entity<SubGridComponent> grid, ref Dictionary<Vector2i, TileAtmos[]> map)
    {
        foreach (var (chunkPos, chunk) in grid.Comp.ChunkEntities)
        {
            var chunkComp = ChunkQuery.Comp(chunk);
            map.Add(chunkPos, chunkComp.AtmosphereMap);
        }
    }

    /// <summary>
    /// Gets all temperature chunks on a grid and adds them to a given dictionary.
    /// </summary>
    /// <remarks>
    /// Remember to clear the dictionary before passing any information into it!
    /// </remarks>
    /// <param name="grid">A grid to find the chunks on.</param>
    /// <param name="map">A cache dictionary to add the chunk data.</param>
    [PublicAPI]
    public void ResolveTemperatureChunkMap(Entity<SubGridComponent> grid, ref Dictionary<Vector2i, TileHeat[]> map)
    {
        foreach (var (chunkPos, chunk) in grid.Comp.ChunkEntities)
        {
            var chunkComp = ChunkQuery.Comp(chunk);
            map.Add(chunkPos, chunkComp.TemperatureMap);
        }
    }

    /// <summary>
    /// Tries to find an atmosphere tile near some other tile.
    /// </summary>
    /// <param name="chunks">A map of all chunks on a grid.</param>
    /// <param name="chunkIndices">Chunk indices that are defined in pair with an index.</param>
    /// <param name="index">Index of a tile inside the chunk.</param>
    /// <param name="dir">Offset vector to try to find the other tile on. For example, a (0,1) vector will return the tile that is a neighbour on the north.</param>
    /// <param name="found">An atmosphere tile that was found.</param>
    /// <returns>True if the atmosphere tile was found.</returns>
    [PublicAPI]
    public bool TryGetAtmosphereTileRelative(
        Dictionary<Vector2i, TileAtmos[]> chunks,
        Vector2i chunkIndices,
        int index,
        Vector2i dir,
        [NotNullWhen(true)] out TileAtmos? found)
    {
        found = null;

        var (targetChunk, targetLocalIndex) = GetTileRelative(chunkIndices, index, dir);

        if (!chunks.TryGetValue(targetChunk, out var chunk))
            return false;

        found = chunk[targetLocalIndex];
        return true;
    }

    /// <summary>
    /// Tries to find a temperature tile near some other tile.
    /// </summary>
    /// <param name="chunks">A map of all chunks on a grid.</param>
    /// <param name="chunkIndices">Chunk indices that are defined in pair with an index.</param>
    /// <param name="index">Index of a tile inside the chunk.</param>
    /// <param name="dir">Offset vector to try to find the other tile on. For example, a (0,1) vector will return the tile that is a neighbour on the north.</param>
    /// <param name="found">A temperature tile that was found.</param>
    /// <returns>True if the temperature tile was found.</returns>
    [PublicAPI]
    public bool TryGetTemperatureTileRelative(
        Dictionary<Vector2i, TileHeat[]> chunks,
        Vector2i chunkIndices,
        int index,
        Vector2i dir,
        [NotNullWhen(true)] out TileHeat? found)
    {
        found = null;

        var (targetChunk, targetLocalIndex) = GetTileRelative(chunkIndices, index, dir);

        if (!chunks.TryGetValue(targetChunk, out var chunk))
            return false;

        found = chunk[targetLocalIndex];
        return true;
    }
}
