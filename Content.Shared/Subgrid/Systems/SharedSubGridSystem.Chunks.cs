using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Atmospherics;
using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature;
using Robust.Shared.Utility;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    /// <summary>
    /// Converts normal position into chunk indices.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    public static Vector2i GetChunkIndices(Vector2 coordinates)
    {
        // Negative coordinates should have offset by 1 because of how coordinates work.
        var x = (int) MathF.Round((coordinates.X >= 0 ? coordinates.X : coordinates.X + 1) / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        var y = (int) MathF.Round((coordinates.Y >= 0 ? coordinates.Y : coordinates.Y + 1) / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        return new Vector2i(x, y);
    }

    /// <summary>
    /// Rounds normal position to the nearest chunk position.
    /// </summary>
    /// <returns></returns>
    public static Vector2 GetChunkPosition(Vector2 coordinates)
        => GetChunkIndices(coordinates) * SystemConstants.PvsChunkSize;

    public static Vector2 ChunkIndicesToPosition(Vector2i indices)
        => indices * SystemConstants.PvsChunkSize;

    public static Vector2 ChunkBoxVector = new Vector2(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize);

    /// <summary>
    /// Converts an (x,y) vector into an index inside a chunk.
    /// </summary>
    /// <param name="relativePos"></param>
    /// <returns></returns>
    public int VectorToIndex(Vector2i relativePos)
    {
        return VectorToIndex(relativePos.X, relativePos.Y);
    }

    public int VectorToIndex(int x, int y)
    {
        DebugTools.Assert(x <= SubGridTileSize);
        DebugTools.Assert(y <= SubGridTileSize);
        return x + (y << (SubGridDivisions + 3));
    }

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

    /// <summary>
    /// A version of <see cref="GetTileRelative"/> that assumes
    /// that the target tile is located inside the same chunk.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    public int GetTileRelativeLocal(int index, Vector2i dir)
    {
        var x = index & (SubGridChunkSize - 1);
        var y = index >> (SubGridDivisions + 3);
        var tx = x + dir.X;
        var ty = y + dir.Y;
        return VectorToIndex(tx, ty);
    }

    public int GetTileRelativeLocal(int index, int dx, int dy)
    {
        var x = index & (SubGridChunkSize - 1);
        var y = index >> (SubGridDivisions + 3);
        var tx = x + dx;
        var ty = y + dy;
        return VectorToIndex(tx, ty);
    }

    public int GetTileAtPosition(Vector2i chunkIndices, Vector2 position)
    {
        var chunkPos = ChunkIndicesToPosition(chunkIndices);
        var delta = chunkPos - position;
        DebugTools.Assert(delta.X >= 0);
        DebugTools.Assert(delta.Y >= 0);
        return GetTileAtPositionRelative(delta + new Vector2(-4f, 4f)); // Aligns the position to the chunks corner
    }

    public int GetTileAtPositionRelative(Vector2 relativePos)
    {
        // Scale the position according to the chunk size
        relativePos *= SubGridTileSize;
        // Round to the nearest position
        var roundedPos = (Vector2i) relativePos.Rounded();
        return VectorToIndex(roundedPos);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// This method assumes that the box is located strictly inside the chunk.
    /// </remarks>
    /// <param name="chunkIndices"></param>
    /// <param name="localBox"></param>
    /// <returns></returns>
    public int[] GetAreaTileIndexesLocal(Vector2i chunkIndices, Box2 localBox)
    {
        var chunkPos = ChunkIndicesToPosition(chunkIndices);
        var box = localBox.Translated(chunkPos + new Vector2(-4f, 4f));
        DebugTools.Assert(box.TopLeft.X == 0);
        DebugTools.Assert(box.TopLeft.Y == 0);
        return GetAreaTileIndexesLocalRelative(box);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="chunkIndices"></param>
    /// <param name="relativeBox"></param>
    /// <remarks>
    /// This method assumes that the box is located strictly inside the chunk.
    /// </remarks>
    /// <returns>An array of indexes in that box area.</returns>
    public int[] GetAreaTileIndexesLocalRelative(Box2 relativeBox)
    {
        var box = (Box2i) relativeBox.Scale(SubGridTileSize).Rounded(0);
        var arr = new int[box.Width * box.Height];

        var corner = VectorToIndex(box.TopLeft);
        for (int i = 0; i < box.Width; i++)
        {
            for (int j = 0; j < box.Height; j++)
            {
                arr[i + j] = GetTileRelativeLocal(corner, i, j);
            }
        }

        return arr;
    }
}
