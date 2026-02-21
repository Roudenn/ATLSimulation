using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Atmospherics;
using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature;
using Robust.Shared.Map;
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

    public static readonly Vector2 ChunkSizeVector = new(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize);

    public static readonly Vector2 HalfChunkSizeVector = new(4f, 4f);

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

    public Vector2i IndexToVector(int index)
    {
        var x = index & (SubGridChunkSize - 1);
        var y = index >> (SubGridDivisions + 3);
        return new Vector2i(x, y);
    }

    public bool TryGetChunk(
        Entity<SubGridComponent?> grid,
        TileRef tile,
        [NotNullWhen(true)] out Entity<SubGridChunkComponent>? chunk)
    {
        return TryGetChunk(grid, tile.GridIndices, out chunk);
    }

    public bool TryGetChunk(
        Entity<SubGridComponent?> grid,
        EntityCoordinates coords,
        [NotNullWhen(true)] out Entity<SubGridChunkComponent>? chunk)
    {
        return TryGetChunk(grid, coords.Position, out chunk);
    }

    public bool TryGetChunk(
        Entity<SubGridComponent?> grid,
        Vector2 position,
        [NotNullWhen(true)] out Entity<SubGridChunkComponent>? chunk)
    {
        chunk = null;
        if (!SubGridQuery.Resolve(grid.Owner, ref grid.Comp))
            return false;

        var ent = grid.Comp.ChunkEntities[GetChunkIndices(position)];
        if (!ChunkQuery.TryComp(ent, out var subGridChunk))
            return false;

        chunk = (ent, subGridChunk);
        return true;
    }

    public void ResolveAtmosphereChunkMap(Entity<SubGridComponent> grid, ref Dictionary<Vector2i, TileAtmosphere[]> map)
    {
        foreach (var (chunkPos, chunk) in grid.Comp.ChunkEntities)
        {
            var chunkComp = ChunkQuery.Comp(chunk);
            map.Add(chunkPos, chunkComp.AtmosphereMap);
        }
    }

    public void ResolveTemperatureChunkMap(Entity<SubGridComponent> grid, ref Dictionary<Vector2i, TileTemperature[]> map)
    {
        foreach (var (chunkPos, chunk) in grid.Comp.ChunkEntities)
        {
            var chunkComp = ChunkQuery.Comp(chunk);
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
    /// Converts local tile index into the MapGrid position.
    /// </summary>
    /// <param name="chunkIndices"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector2 GetPositionFromIndex(Vector2i chunkIndices, int index)
    {
        // Convert the position to world units
        var localPos = IndexToVector(index) / (float) SubGridTileSize;
        // Set the position to be centered around the chunk origin
        var relative = localPos - HalfChunkSizeVector;
        // Rotate it by 90 degrees to convert into X+ right and Y+ up coordinates
        var rotated = new Vector2(relative.Y, -relative.X);

        return ChunkIndicesToPosition(chunkIndices) + rotated;
    }

    /// <summary>
    /// A version of <see cref="GetTileRelative"/> that assumes
    /// that the target tile is located inside the same chunk.
    /// </summary>
    /// <remarks>
    /// Use <see cref="GetTileRelative"/> unless you are sure that the target has to be in the same chunk!
    /// </remarks>
    /// <param name="index">Index of the current tile.</param>
    /// <param name="dir">Direction</param>
    /// <returns>Index of the target tile.</returns>
    public int GetTileRelativeLocal(int index, Vector2i dir)
    {
        return GetTileRelativeLocal(index, dir.X, dir.Y);
    }

    /// <inheritdoc cref="GetTileRelativeLocal(int, Vector2i)"/>
    public int GetTileRelativeLocal(int index, int dx, int dy)
    {
        var pos = IndexToVector(index);
        var tx = pos.X + dx;
        var ty = pos.Y + dy;
        return VectorToIndex(tx, ty);
    }

    /// <summary>
    /// Gets index of a tile on a specified MapGrid position.
    /// </summary>
    /// <param name="position">MapGrid position of a target tile.</param>
    /// <returns>Index of the closest subgrid tile to the target position.</returns>
    public int GetTileAtPosition(Vector2 position)
    {
        return GetTileAtRelativePosition(GetRelativeChunkPosition(position));
    }

    public Vector2 GetRelativeChunkPosition(Vector2 position)
    {
        // Position relative to the center of the chunk
        var relative = position - GetChunkPosition(position);
        // Rotate it by 90 degrees to convert into X+ right and Y+ down coordinates
        var rotated = new Vector2(-relative.Y, relative.X);
        // Move the origin of the vector to the top-left corner of the chunk
        return rotated + HalfChunkSizeVector;
    }

    public int GetTileAtRelativePosition(Vector2 relativePos)
    {
        // Scale the position according to the chunk size and round it
        var roundedPos = (Vector2i) (relativePos * SubGridChunkSize).Rounded();
        return VectorToIndex(roundedPos);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public int[] GetAreaTileIndexesAtTile(Vector2i gridIndices, Vector2 tileSizeVector)
    {
        var vec1 = GetRelativeChunkPosition(gridIndices);
        var vec2 = GetRelativeChunkPosition(gridIndices + tileSizeVector);
        return GetAreaTileIndexesLocalRelative(vec1, vec2);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// This method assumes that the box is located strictly inside the chunk.
    /// </remarks>
    /// <returns>An array of indexes in that box area.</returns>
    public int[] GetAreaTileIndexesLocalRelative(Vector2 topLeft, Vector2 bottomRight)
    {
        var topLeftRelative = (Vector2i) (topLeft * SubGridTileSize).Rounded();
        var bottomRightRelative = (Vector2i) (bottomRight * SubGridTileSize).Rounded();
        var width = Math.Abs(topLeftRelative.X - bottomRightRelative.X);
        var height = Math.Abs(bottomRightRelative.Y - topLeftRelative.Y);

        var arr = new int[width * height];
        var corner = VectorToIndex(topLeftRelative);
        var count = 0; // probably could be better than another counter but idc
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                arr[count] = GetTileRelativeLocal(corner, j, i);
                count++;
            }
        }

        return arr;
    }
}
