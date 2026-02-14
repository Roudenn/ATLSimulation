using System.Numerics;
using Content.Shared.Atmospherics;
using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.SubGrid;

public sealed partial class SubGridSystem
{
    private void InitializeChunks()
    {
        SubscribeLocalEvent<GridInitializeEvent>(OnGridInit);
        //SubscribeLocalEvent<SubGridComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SubGridComponent, TileChangedEvent>(OnChanged);
        SubscribeLocalEvent<SubGridComponent, EntityTerminatingEvent>(OnTerminating);

        SubscribeLocalEvent<SubGridChunkComponent, MapInitEvent>(OnChunkInit);
        SubscribeLocalEvent<SubGridChunkComponent, EntityTerminatingEvent>(OnChunkDeleted);
    }

    /// <summary>
    /// Converts normal position into chunk indices.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    public static Vector2i GetChunkIndices(Vector2 coordinates)
    {
        // Negative coordinates should have offset by 1 because of how coordinates work.
        var x = (int) MathF.Round((coordinates.X >= 0 ? coordinates.X : coordinates.X + 1) / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        var y = (int)MathF.Round((coordinates.Y >= 0 ? coordinates.Y : coordinates.Y + 1) / SystemConstants.PvsChunkSize, MidpointRounding.AwayFromZero);
        return new Vector2i(x, y);
    }

    /// <summary>
    /// Rounds normal position to the nearest chunk position.
    /// </summary>
    /// <returns></returns>
    public static Vector2 GetChunkPosition(Vector2 coordinates)
        => GetChunkIndices(coordinates) * SystemConstants.PvsChunkSize;

    private void OnGridInit(GridInitializeEvent ev)
    {
        if (HasComp<MapComponent>(ev.EntityUid))
            return;

        EnsureComp<SubGridComponent>(ev.EntityUid);
    }

    private void OnTerminating(Entity<SubGridComponent> ent, ref EntityTerminatingEvent args)
    {
        foreach (var (_, uid) in ent.Comp.ChunkEntities)
        {
            QueueDel(uid);
        }
    }

    /*private void OnInit(Entity<SubGridComponent> ent, ref ComponentInit args)
    {
        EnsureSubGrid(ent);
    }*/

    private void OnChanged(Entity<SubGridComponent> ent, ref TileChangedEvent args)
    {
        // TODO this doesn't support grid splitting
        foreach (var change in args.Changes)
        {
            EnsureSubGridChunk(ent, change.GridIndices, out _);
        }
    }

    private void OnChunkInit(Entity<SubGridChunkComponent> ent, ref MapInitEvent args)
    {
        if (!_mapGridQuery.TryComp(ent.Comp.ParentGrid, out var mapGridComp))
        {
            DebugTools.Assert("SubGrid chunk was initialized without a parent grid!");
            return;
        }

        //InitializeChunkAtmos((ent.Owner, ent.Comp, xform), (xform.GridUid.Value, mapGridComp));
    }

    /*private void InitializeChunkAtmos(Entity<SubGridChunkComponent, TransformComponent> ent, Entity<MapGridComponent> grid)
    {
        var xform = ent.Comp2;

        var atmos = new TileAtmosphere[SubGridChunkArea];
        var chunkArea = Box2.CenteredAround(ent., new Vector2(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize));

        for (var i = 0; i < atmos.Length; i++)
        {
            var subTile = atmos[i];

        }
    }*/

    private void OnChunkDeleted(Entity<SubGridChunkComponent> ent, ref EntityTerminatingEvent args)
    {
        if (!_subGridQuery.TryComp(ent.Comp.ParentGrid, out var subGrid))
            return;

        subGrid.ChunkEntities.Remove(GetChunkIndices(ent.Comp.ChunkIndices));
    }

    /// <summary>
    /// Ensures that every single tile of the mapgrid is covered by the subgrid.
    /// </summary>
    /// <remarks>
    /// This may cause huge lag spikes when called in the simulation runtime!
    /// If you need to only update an area of a grid, use <see cref="EnsureSubGridChunk"/> instead.
    /// </remarks>
    /// <param name="grid">The target grid to ensure all tiles on.</param>
    private void EnsureSubGrid(Entity<SubGridComponent, MapGridComponent?> grid)
    {
        // TODO: this is slow, would be better to make larger tile steps.
        // This method shouldn't be called on runtime for now, since grid splitting is not supported yet.

        if (!Resolve(grid.Owner, ref grid.Comp2))
            return;

        // Go through all tiles to make sure that grid is fully covered.
        var positions = new HashSet<Vector2i>();
        var tiles = _mapSystem.GetAllTilesEnumerator(grid.Owner, grid.Comp2);
        while (tiles.MoveNext(out var tile))
        {
            positions.Add(tile.Value.GridIndices);
        }

        foreach (var pos in positions)
        {
            EnsureSubGridChunk(grid, pos, out _);
        }

        Log.Info($"Successfully initialized SubGrid on grid {ToPrettyString(grid.Owner)}. Total amount of chunks: {grid.Comp1.ChunkEntities.Count}");
    }

    /// <summary>
    /// Ensures that a given tile position has an assigned subgrid chunk.
    /// Returns either a new chunk or an already existing one that contains that tile.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="gridIndices"></param>
    /// <param name="chunk"></param>
    private void EnsureSubGridChunk(Entity<SubGridComponent> grid, Vector2i gridIndices, out EntityUid chunk)
    {
        if (grid.Comp.ChunkEntities.TryGetValue(GetChunkIndices(gridIndices), out chunk))
            return;

        chunk = SpawnSubGridChunk(grid, gridIndices);
    }

    /// <summary>
    /// Spawns a subgrid chunk entity on a grid at a specified position, aligned to chunk indices.
    /// </summary>
    /// <param name="grid">The relative grid.</param>
    /// <param name="gridIndices">The grid indices that are inside the chunk.</param>
    /// <returns>The spawned subgrid chunk.</returns>
    private EntityUid SpawnSubGridChunk(Entity<SubGridComponent> grid, Vector2i gridIndices)
    {
        var pos = GetChunkPosition(gridIndices);
        var chunk = Spawn(null, new EntityCoordinates(grid, pos));
        var chunkComp = EnsureComp<SubGridChunkComponent>(chunk);

        // TODO this is shitcode
        chunkComp.ParentGrid = grid.Owner;
        chunkComp.ChunkIndices = GetChunkIndices(gridIndices);

        grid.Comp.ChunkEntities.Add(chunkComp.ChunkIndices, chunk);
        Log.Info($"Added chunk {ToPrettyString(chunk)} to grid {ToPrettyString(grid)} with chunkIndices {chunkComp.ChunkIndices}");
        return chunk;
    }
}
