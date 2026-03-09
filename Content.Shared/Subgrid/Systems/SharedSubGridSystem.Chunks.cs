using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Constants;
using Content.Shared.Maths;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    /// <summary>
    /// Converts Grid position into chunk indices.
    /// </summary>
    /// <param name="coordinates">Grid position that we are trying to find the chunk indices for.</param>
    /// <returns>Nearest chunk indices to the given coordinates.</returns>
    public static Vector2i GetChunkIndices(Vector2 coordinates)
    {
        var x = (int) MathF.Round(coordinates.X / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        var y = (int) MathF.Round(coordinates.Y / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        return new Vector2i(x, y);
    }

    public static Vector2i GetChunkIndicesTile(Vector2 gridIndices)
    {
        // Negative coordinates should have offset by 1 because Grid Indices are stored in the bottom left corners.
        var x = (int) MathF.Round((gridIndices.X >= 0f ? gridIndices.X : gridIndices.X + 1f) / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        var y = (int) MathF.Round((gridIndices.Y >= 0f ? gridIndices.Y : gridIndices.Y + 1f) / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        return new Vector2i(x, y);
    }

    /// <summary>
    /// Rounds normal position to the nearest chunk position.
    /// </summary>
    /// <param name="coordinates">Grid position that we are trying to find the chunk position for.</param>
    /// <returns>Nearest chunk position to the given coordinates.</returns>
    public static Vector2 GetChunkPosition(Vector2 coordinates)
        => GetChunkIndices(coordinates) * SystemConstants.PvsChunkSize;

    public static Vector2 ChunkIndicesToPosition(Vector2i indices)
        => indices * SystemConstants.PvsChunkSize;

    /// <summary>
    /// A vector that has a length and width equal to the subgrid chunk size.
    /// </summary>
    public static readonly Vector2 ChunkSizeVector = new(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize);

    /// <summary>
    /// Half of the <see cref="ChunkSizeVector"/>.
    /// </summary>
    public static readonly Vector2 HalfChunkSizeVector = new(4f, 4f);

    public static Box2 ChunkBoxAtIndices(Vector2i chunkIndices)
    {
        return Box2.CenteredAround(
            chunkIndices * SystemConstants.PvsChunkSize,
            new Vector2(SystemConstants.PvsChunkSize));
    }

    /// <inheritdoc cref="VectorToIndex(int, int)"/>
    public int VectorToIndex(Vector2i relativePos)
    {
        return VectorToIndex(relativePos.X, relativePos.Y);
    }

    /// <summary>
    /// Converts an (x,y) vector into an index inside a chunk.
    /// </summary>
    /// <returns>
    /// An index in a 1D array of the chunk that represents a tile at a given (x, y) local position.
    /// </returns>
    public int VectorToIndex(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, SubGridChunkSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, SubGridChunkSize);
        return x + (y << (SubGridDivisions + 3));
    }

    /// <summary>
    /// Converts an index into a (x,y) local position inside a chunk.
    /// </summary>
    /// <param name="index">An index of an element of a 1D array of a subgrid chunk.</param>
    /// <returns>An (x,y) vector local position of that tile inside a chunk.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If index is greater than or equal to the total amount of subgrid tiles in a single chunk (the index is out-of-bounds)
    /// </exception>
    public Vector2i IndexToVector(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, SubGridChunkArea);
        var x = index & (SubGridChunkSize - 1);
        var y = index >> (SubGridDivisions + 3);
        return new Vector2i(x, y);
    }

    /// <inheritdoc cref='TryGetChunk(Entity{SubGridComponent?}, Vector2, out Entity{SubGridChunkComponent}?)'/>
    public bool TryGetChunk(
        Entity<SubGridComponent?> grid,
        TileRef tile,
        [NotNullWhen(true)] out Entity<SubGridChunkComponent>? chunk)
    {
        return TryGetChunk(grid, tile.GridIndices, out chunk);
    }

    /// <inheritdoc cref='TryGetChunk(Entity{SubGridComponent?}, Vector2, out Entity{SubGridChunkComponent}?)'/>
    public bool TryGetChunk(
        Entity<SubGridComponent?> grid,
        Vector2i gridIndices,
        [NotNullWhen(true)] out Entity<SubGridChunkComponent>? chunk)
    {
        chunk = null;
        if (!SubGridQuery.Resolve(grid.Owner, ref grid.Comp)
            || !grid.Comp.ChunkEntities.TryGetValue(GetChunkIndicesTile(gridIndices), out var ent)
            || !ChunkQuery.TryComp(ent, out var subGridChunk))
            return false;

        chunk = (ent, subGridChunk);
        return true;
    }

    /// <summary>
    /// A helper method that tries to get a subgrid chunk at a given grid at specific coordinates.
    /// </summary>
    /// <param name="grid">A grid to find the subgrid chunk on.</param>
    /// <param name="position">Grid position to try to find the nearest chunk for.</param>
    /// <param name="chunk">The found subgrid chunk that is nearest to the given position.</param>
    /// <returns>True if the chunk was found.</returns>
    public bool TryGetChunk(
        Entity<SubGridComponent?> grid,
        Vector2 position,
        [NotNullWhen(true)] out Entity<SubGridChunkComponent>? chunk)
    {
        chunk = null;
        if (!SubGridQuery.Resolve(grid.Owner, ref grid.Comp)
            || !grid.Comp.ChunkEntities.TryGetValue(GetChunkIndices(position), out var ent)
            || !ChunkQuery.TryComp(ent, out var subGridChunk))
            return false;

        chunk = (ent, subGridChunk);
        return true;
    }

    /// <summary>
    /// Finds relative position of a chunk and an index inside that chunk
    /// relative to some another index inside the given chunk.
    /// A helper method that allows to move inside 1D array as if it was a 2D array.
    /// </summary>
    /// <param name="chunkIndices">Relative chunk position.</param>
    /// <param name="index">Index of a tile inside the chunk at <see cref="chunkIndices"/>.</param>
    /// <param name="dir">The relative position of a target tile from <see cref="chunkIndices"/> and <see cref="index"/>.</param>
    /// <returns></returns>
    public (Vector2i, int) GetTileRelative(Vector2i chunkIndices, int index, Vector2i dir)
    {
        var x = index & (SubGridChunkSize - 1); // same as (index % ChunkSize)
        var y = index >> (SubGridDivisions + 3); // 3 because of the minimal chunk size being 8

        // Get the new index
        var tx = x + dir.X;
        var ty = y + dir.Y;

        // Get the target chunk that should contain the new index
        var targetChunkX = chunkIndices.X + (tx >> (SubGridDivisions + 3));
        var targetChunkY = chunkIndices.Y + (ty >> (SubGridDivisions + 3));

        // Calculate the new local index:
        // 1) Wrap the coordinate by using &
        // 2) Divide Y by ChunkSize (since Y represents current row)
        // 3) Add X and Y up to get the result
        var targetLocalIndex = (tx & (SubGridChunkSize - 1)) | ((ty & (SubGridChunkSize - 1)) << (SubGridDivisions + 3));

        return (new Vector2i(targetChunkX, targetChunkY), targetLocalIndex);
    }

    /// <summary>
    /// Converts local tile index into the grid position.
    /// </summary>
    /// <param name="chunkIndices">Chunk indices of a chunk that an index is stored in.</param>
    /// <param name="index">Index of a tile that we are trying to get a position for.</param>
    /// <returns></returns>
    public Vector2 GetPositionFromIndex(Vector2i chunkIndices, int index)
    {
        // Convert the position to world units
        var localPos = IndexToVector(index) / (float) SubGridTileSize;
        // Set the position to be centered around the chunk origin
        var relative = localPos - HalfChunkSizeVector;
        return ChunkIndicesToPosition(chunkIndices) + relative;
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
    /// Gets an index of a tile on a specified grid position.
    /// </summary>
    /// <param name="position">Grid position of a target tile.</param>
    /// <returns>Index of the closest subgrid tile to the target position.</returns>
    public int GetTileAtPosition(Vector2 position)
    {
        return GetTileAtRelativePosition(GetRelativeChunkPosition(position));
    }

    public Vector2 GetRelativeChunkPosition(Vector2 position)
    {
        // Position relative to the center of the chunk
        var relative = position - GetChunkPosition(position);
        // Move the origin of the vector to the top-left corner of the chunk
        return relative + HalfChunkSizeVector;
    }

    public Vector2 GetRelativeChunkPosition(Vector2 position, Vector2i chunkIndices)
    {
        // Position relative to the center of the chunk
        var relative = position - chunkIndices * SystemConstants.PvsChunkSize;
        // Move the origin of the vector to the bottom-left corner of the chunk
        return relative + HalfChunkSizeVector;
    }

    public int GetTileAtRelativePosition(Vector2 relativePos)
    {
        // Scale the position according to the chunk size and round it
        var roundedPos = (Vector2i) (relativePos * SubGridTileSize).Rounded();
        return VectorToIndex(roundedPos);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="aabb"></param>
    /// <returns></returns>
    public HashSet<(Vector2i, int[])> GetTileIndexesWorld(Box2 aabb)
    {
        var set = new HashSet<(Vector2i, int[])>();

        // The trivial case.
        var chunk = GetChunkIndices(aabb.Center);
        var chunkBox = ChunkBoxAtIndices(chunk);
        if (chunkBox.Contains(aabb))
        {
            var vec1 = GetRelativeChunkPosition(aabb.BottomLeft, chunk);
            var vec2 = GetRelativeChunkPosition(aabb.TopRight, chunk);
            set.Add((chunk, GetAreaTileIndexesRelativeChunk(vec1, vec2)));
            return set;
        }

        // Split the box into boxes that fit inside their chunks.
        var boxes = AABBIntersectRecursiveChunks(aabb);
        foreach (var box in boxes)
        {
            // The box is guaranteed to be inside the chunk,
            // but to say for sure in which chunk the box is actually in, it has to look at it's center.
            var chunkIndices = GetChunkIndices(box.Center);
            var vec1 = GetRelativeChunkPosition(box.BottomLeft, chunkIndices);
            var vec2 = GetRelativeChunkPosition(box.TopRight, chunkIndices);
            var indexes = GetAreaTileIndexesRelativeChunk(vec1, vec2);
            set.Add((chunkIndices, indexes));
        }

        return set;
    }

    private List<Box2> AABBIntersectRecursiveChunks(Box2 aabb, int count = 0)
    {
        if (count > 10)
            Log.Warning("uummm");

        // Make sure that the resulting boxes don't intersect with other chunks.
        var bottomLeftChunkInter = GetChunkIndices(aabb.BottomLeft);
        var topRightChunkInter = GetChunkIndices(aabb.TopRight);
        var chunkIndices = GetChunkIndices(aabb.Center);

        if (bottomLeftChunkInter == topRightChunkInter)
            return new List<Box2> { aabb }; // This box is contained inside a chunk.

        var list = new List<Box2>();
        var chunkBox = ChunkBoxAtIndices(chunkIndices);
        var intersected = aabb.AABBIntersection(chunkBox);

        // Check if the chunks were connected diagonally or were far way.
        if (chunkBox.Contains(aabb) // First condition handles cases with corners.
            || Vector2.Sum(Vector2.Abs(topRightChunkInter - bottomLeftChunkInter)) < 2f)
        {
            // All boxes must have been contained now.
            list.AddRange(intersected);
            return list;
        }

        foreach (var intersect in intersected)
        {
            count++;
            var boxes = AABBIntersectRecursiveChunks(intersect, count);
            list.AddRange(boxes);
        }

        return list;
    }

    /// <summary>
    /// Gets all tile indexes at specified grid indices.
    /// </summary>
    /// <param name="chunkIndices">Chunk indices to find the indexes in.</param>
    /// <param name="gridIndices">Grid indices of a target tile.</param>
    /// <param name="tileSizeVector">Tile size vector, usually obtained from the <see cref="MapGridComponent"/>.</param>
    /// <returns>An array that contains the found indexes, stored left to right, top to bottom.</returns>
    public int[] GetAreaTileIndexesAtTile(Vector2i chunkIndices, Vector2i gridIndices, Vector2 tileSizeVector)
    {
        var vec1 = GetRelativeChunkPosition(gridIndices, chunkIndices);
        var vec2 = GetRelativeChunkPosition(gridIndices + tileSizeVector, chunkIndices);
        return GetAreaTileIndexesRelativeChunk(vec1, vec2);
    }

    /// <summary>
    /// Gets all tile indexes between two relative chunk positions.
    /// </summary>
    /// <remarks>
    /// This method assumes that both positions are located strictly inside the same chunk.
    /// </remarks>
    /// <param name="bottomLeft">Bottom left coordinates.</param>
    /// <param name="topRight">Top right coordinates.</param>
    /// <returns>An array that contains the found indexes, stored left to right, top to bottom.</returns>
    // TODO make a version of this method that supports coordinates in multiple chunks at once
    public int[] GetAreaTileIndexesRelativeChunk(Vector2 bottomLeft, Vector2 topRight)
    {
        var bottomLeftIndex = (Vector2i) (bottomLeft * SubGridTileSize).Rounded();
        var topRightIndex = (Vector2i) (topRight * SubGridTileSize).Rounded();
        return GetAreaTileIndexes(bottomLeftIndex, topRightIndex);
    }

    /// <summary>
    /// Gets all tile indexes between two points inside the chunk 1D array.
    /// </summary>
    /// <remarks>
    /// This method assumes that both points are located strictly inside the same chunk.
    /// </remarks>
    /// <param name="bottomLeftIndex">Bottom left point.</param>
    /// <param name="topRightIndex">Top right point.</param>
    /// <returns>An array that contains the found indexes, stored left to right, top to bottom.</returns>
    public int[] GetAreaTileIndexes(Vector2i bottomLeftIndex, Vector2i topRightIndex)
    {
        var width = Math.Abs(bottomLeftIndex.X - topRightIndex.X);
        var height = Math.Abs(topRightIndex.Y - bottomLeftIndex.Y);

        var arr = new int[width * height];
        var corner = VectorToIndex(bottomLeftIndex);
        var count = 0; // probably could be better than another counter but idc
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                arr[count] = GetTileRelativeLocal(corner, j, i);
                if (arr[count] < 0)
                    Log.Warning("WTF??");
                count++;
            }
        }

        return arr;
    }

    /// <summary>
    /// Gets corner indexes of o
    /// </summary>
    /// <param name="chunkIndices"></param>
    /// <param name="gridIndices"></param>
    /// <param name="tileSizeVector"></param>
    /// <returns></returns>
    public Box2i GetTileCornerIndexes(Vector2i chunkIndices, Vector2i gridIndices, Vector2 tileSizeVector)
    {
        var vec1 = GetRelativeChunkPosition(gridIndices, chunkIndices);
        var vec2 = GetRelativeChunkPosition(gridIndices + tileSizeVector, chunkIndices);
        var bottomLeftIndex = (Vector2i) (vec1 * SubGridTileSize).Rounded() - Vector2i.One;
        var topRightIndex = (Vector2i) (vec2 * SubGridTileSize).Rounded() - Vector2i.One;
        return new Box2i(bottomLeftIndex, topRightIndex);
    }
}
