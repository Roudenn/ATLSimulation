using System.Numerics;
using Content.Shared.Atmospherics;
using Content.Shared.Constants;
using Content.Shared.Maps;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.SubGrid;

public sealed partial class SubGridSystem
{
    private void InitializeChunks()
    {
        SubscribeLocalEvent<GridInitializeEvent>(OnGridInit);

        SubscribeLocalEvent<SubGridComponent, TileChangedEvent>(OnGridChanged);
        //SubscribeLocalEvent<SubGridComponent, MoveEvent>(OnGridMove); // TODO handle grid movement
        // TODO handle grid splitting
        SubscribeLocalEvent<SubGridComponent, EntityTerminatingEvent>(OnGridTerminating);

        SubscribeLocalEvent<SubGridChunkComponent, MapInitEvent>(OnChunkInit);
        SubscribeLocalEvent<SubGridChunkComponent, EntityTerminatingEvent>(OnChunkDeleted);
    }

    private void OnGridInit(GridInitializeEvent ev)
    {
        if (HasComp<MapComponent>(ev.EntityUid))
            return;

        EnsureComp<SubGridComponent>(ev.EntityUid);
    }

    private void OnGridChanged(Entity<SubGridComponent> ent, ref TileChangedEvent args)
    {
        // TODO this doesn't support grid splitting
        foreach (var change in args.Changes)
        {
            if (change.OldTile.TypeId == 0) // If space changed to something else, add chunks
                EnsureSubGridChunkWithNeighbours(ent, change.GridIndices);
            else if (change.NewTile.TypeId == 0) // Otherwise check the chunks to be removed
                TryRemoveSubGridChunkWithNeighbours(ent, change.GridIndices);
        }
    }

    private void OnGridTerminating(Entity<SubGridComponent> ent, ref EntityTerminatingEvent args)
    {
        foreach (var (_, uid) in ent.Comp.ChunkEntities)
        {
            QueueDel(uid);
        }
    }

    private void OnChunkInit(Entity<SubGridChunkComponent> ent, ref MapInitEvent args)
    {
        if (!_mapGridQuery.TryComp(ent.Comp.ParentGrid, out var mapGridComp))
        {
            DebugTools.Assert("SubGrid chunk was initialized without a parent grid!");
            return;
        }

        InitializeChunkAtmos((ent.Owner, ent.Comp), (ent.Comp.ParentGrid, mapGridComp));
        InitializeChunkTemperature((ent.Owner, ent.Comp), (ent.Comp.ParentGrid, mapGridComp));
    }

    private void InitializeChunkAtmos(Entity<SubGridChunkComponent> ent, Entity<MapGridComponent> grid)
    {
        var mixture = _atmosGridQuery.CompOrNull(grid.Owner)?.Mixture ?? _atmos.GetSpaceMixture();
        DebugTools.Assert(mixture.Immutable);

        var atmos = new TileAtmosphere[SubGridChunkArea];
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkBoxVector);

        var tiles = _mapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp, chunkArea);

        while (tiles.MoveNext(out var tile))
        {
            // TODO This can be made better, probably by taking a similar method to grid fixtures

            var tileBox = new Box2i(tile.GridIndices, tile.GridIndices + Vector2i.One);
            var subTiles = GetAreaTileIndexesLocal(ent.Comp.ChunkIndices, tileBox);
            foreach (var index in subTiles)
            {
                atmos[index] = new TileAtmosphere(mixture);
            }

            // At last, the tile has to check if it is located near space, and add a boundary layer of atmosphere tiles.
            foreach (var dir in DirectionsWithDiagonals)
            {
                var tileRef = _mapSystem.GetTileRef(grid.Owner, grid.Comp, tile.GridIndices + dir);
                var tileData = (ContentTileDefinition) _tileDefMan[tileRef.Tile.TypeId];
                if (!tileData.MapAtmosphere)
                    continue;

                // TODO generate boundary tiles here
            }
        }

        ent.Comp.AtmosphereMap = atmos;
    }

    private void InitializeChunkTemperature(Entity<SubGridChunkComponent> ent, Entity<MapGridComponent> grid)
    {
        var temperatureMap = new TileTemperature[SubGridChunkArea];
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkBoxVector);

        var tiles = _mapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp, chunkArea);

        while (tiles.MoveNext(out var tile))
        {
            // TODO This can be made better, probably by taking a similar method to grid fixtures

            float? heatCapacity = null;
            float? temperature = null;

            // Try to get the anchored wall entity on this tile
            var ents = _mapSystem.GetAnchoredEntitiesEnumerator(grid.Owner, grid.Comp, tile.GridIndices);
            while (ents.MoveNext(out var anchored))
            {
                if (!_temperatureQuery.TryComp(anchored, out var tempContainer)
                    || !_materialQuery.TryComp(anchored, out var materialComp))
                    continue;

                var material = _proto.Index(materialComp.Material);
                temperature = tempContainer.StartingTemperature;
                heatCapacity = material.SpecificHeatCapacity * SubGridTileVolume * material.Density;
                break;
            }

            if (heatCapacity == null || temperature == null)
                continue;

            var tileBox = new Box2i(tile.GridIndices, tile.GridIndices + Vector2i.One);
            var subTiles = GetAreaTileIndexesLocal(ent.Comp.ChunkIndices, tileBox);
            foreach (var index in subTiles)
            {
                temperatureMap[index] = new TileTemperature(heatCapacity.Value, temperature.Value);
            }
        }

        ent.Comp.TemperatureMap = temperatureMap;
    }

    private void OnChunkDeleted(Entity<SubGridChunkComponent> ent, ref EntityTerminatingEvent args)
    {
        if (TerminatingOrDeleted(ent.Comp.ParentGrid)
            || !_subGridQuery.TryComp(ent.Comp.ParentGrid, out var subGrid))
            return;

        subGrid.ChunkEntities.Remove(ent.Comp.ChunkIndices);
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

        if (!_mapGridQuery.Resolve(grid.Owner, ref grid.Comp2))
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
            EnsureSubGridChunkWithNeighbours(grid, pos);
        }

        Log.Info($"Successfully initialized SubGrid on grid {ToPrettyString(grid.Owner)}. Total amount of chunks: {grid.Comp1.ChunkEntities.Count}");
    }

    /// <summary>
    /// Ensures that a given tile position and also all neighbours in 8 directions
    /// have an assigned subgrid chunk.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="gridIndices"></param>
    private void EnsureSubGridChunkWithNeighbours(Entity<SubGridComponent> grid, Vector2i gridIndices)
    {
        EnsureSubGridChunk(grid, gridIndices);

        foreach (var dir in DirectionsWithDiagonals)
        {
            EnsureSubGridChunk(grid, gridIndices + dir);
        }
    }

    /// <summary>
    /// Ensures that a given tile position has an assigned subgrid chunk.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="gridIndices"></param>
    private void EnsureSubGridChunk(Entity<SubGridComponent> grid, Vector2i gridIndices)
    {
        if (grid.Comp.ChunkEntities.ContainsKey(GetChunkIndices(gridIndices)))
            return;

        SpawnSubGridChunk(grid, gridIndices);
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

    private void TryRemoveSubGridChunkWithNeighbours(Entity<SubGridComponent, MapGridComponent?> grid, Vector2i gridIndices)
    {
        TryRemoveSubGridChunk(grid, gridIndices);

        foreach (var dir in DirectionsWithDiagonals)
        {
            TryRemoveSubGridChunk(grid, gridIndices + dir);
        }
    }

    /// <summary>
    /// Checks if chunk at the given position can be safely removed,
    /// as if it doesn't have any tiles inside, and it doesn't have any neighbouring tiles.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool TryRemoveSubGridChunk(Entity<SubGridComponent, MapGridComponent?> grid, Vector2 position)
    {
        if (!_mapGridQuery.Resolve(grid.Owner, ref grid.Comp2))
            return false;

        // Check 10x10 box around the center of the chunk for any tiles that are not empty.
        var box = Box2.CenteredAround(GetChunkPosition(position),
            new Vector2(SystemConstants.PvsChunkSize + 2, SystemConstants.PvsChunkSize + 2));

        var tiles = _mapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, box);
        while (tiles.MoveNext(out _))
        {
            // Even a single iteration means that there are some tiles here, and the chunk should stay.
            return false;
        }

        var indices = GetChunkIndices(position);
        var chunk = grid.Comp1.ChunkEntities[indices];
        QueueDel(chunk);
        Log.Info($"Removing chunk {ToPrettyString(chunk)} at chunk indices {indices} on grid {ToPrettyString(grid)}");
        return true;
    }
}
