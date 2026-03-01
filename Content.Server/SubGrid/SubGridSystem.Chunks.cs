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
        SubscribeLocalEvent<SubGridComponent, MapInitEvent>(OnMapInit);
        //SubscribeLocalEvent<SubGridComponent, MoveEvent>(OnGridMove); // TODO handle grid movement
        // TODO handle grid splitting
        SubscribeLocalEvent<SubGridComponent, EntityTerminatingEvent>(OnGridTerminating);

        SubscribeLocalEvent<SubGridChunkComponent, SubGridChunkInitializeEvent>(OnChunkInit);
        SubscribeLocalEvent<SubGridChunkComponent, EntityTerminatingEvent>(OnChunkDeleted);
    }

    private void OnGridInit(GridInitializeEvent ev)
    {
        if (HasComp<MapComponent>(ev.EntityUid))
            return;

        EnsureComp<SubGridComponent>(ev.EntityUid);
    }

    private void OnMapInit(Entity<SubGridComponent> ent, ref MapInitEvent args)
    {
        EnsureSubGrid(ent);
    }

    private void OnGridChanged(Entity<SubGridComponent> ent, ref TileChangedEvent args)
    {
        if (Paused(ent.Owner))
            return; // We don't care about it until we are initialized

        // TODO this doesn't support grid splitting
        foreach (var change in args.Changes)
        {
            if (change.OldTile.TypeId == 0)
            {
                // If space changed to something else, add chunks
                EnsureSubGridChunkWithNeighbours(ent, change.GridIndices);

                if (!TryGetChunk(ent.AsNullable(), change.GridIndices, out var chunk))
                    continue;

                InitializeAtmosAtTile(args.Entity, change.GridIndices, ref chunk.Value.Comp.AtmosphereMap, chunk.Value.Comp.ChunkIndices);
                Dirty(chunk.Value); // TODO optimization
            }
            else if (change.NewTile.TypeId == 0)
            {
                // Otherwise check the chunks to be removed
                TryRemoveSubGridChunkWithNeighbours(ent, change.GridIndices);
            }
        }

        Dirty(ent); // TODO optimization
    }

    private void OnGridTerminating(Entity<SubGridComponent> ent, ref EntityTerminatingEvent args)
    {
        foreach (var (_, uid) in ent.Comp.ChunkEntities)
        {
            QueueDel(uid);
        }
    }

    private void OnChunkInit(Entity<SubGridChunkComponent> ent, ref SubGridChunkInitializeEvent args)
    {
        if (!MapGridQuery.TryComp(ent.Comp.ParentGrid, out var mapGridComp))
        {
            DebugTools.Assert("SubGrid chunk was initialized without a parent grid!");
            return;
        }

        InitializeChunkAtmos((ent.Owner, ent.Comp), (ent.Comp.ParentGrid, mapGridComp));
        InitializeChunkTemperature((ent.Owner, ent.Comp), (ent.Comp.ParentGrid, mapGridComp));
        Dirty(ent);
    }

    private void InitializeChunkAtmos(Entity<SubGridChunkComponent> ent, Entity<MapGridComponent> grid)
    {
        var atmos = new TileAtmos[SubGridChunkArea];
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkSizeVector);

        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp, chunkArea);

        while (tiles.MoveNext(out var tile))
        {
            InitializeAtmosAtTile(grid, tile.GridIndices, ref atmos, ent.Comp.ChunkIndices, true);
        }

        ent.Comp.AtmosphereMap = atmos;
    }

    private void InitializeAtmosAtTile(
        Entity<MapGridComponent> grid,
        Vector2i gridIndices,
        ref TileAtmos[] atmos,
        Vector2i chunkIndices,
        bool gridMixture = false)
    {
        // TODO add airtight stuff
        var mixture = gridMixture ? _atmos.GetGridMixture(grid.Owner) : _atmos.GetSpaceMixture();

        var subTiles = GetAreaTileIndexesAtTile(chunkIndices, gridIndices, grid.Comp.TileSizeVector);
        foreach (var index in subTiles)
        {
            atmos[index] = new TileAtmos(mixture);
        }

        // At last, the tile has to check if it is located near space, and add a boundary layer of atmosphere tiles.
        foreach (var dir in DirectionsWithDiagonals)
        {
            var tileRef = MapSystem.GetTileRef(grid.Owner, grid.Comp, gridIndices + dir);
            var tileData = (ContentTileDefinition) _tileDefMan[tileRef.Tile.TypeId];
            if (!tileData.MapAtmosphere)
                continue;

            // TODO generate boundary tiles here
        }
    }

    private void InitializeChunkTemperature(Entity<SubGridChunkComponent> ent, Entity<MapGridComponent> grid)
    {
        var temperatureMap = new TileHeat[SubGridChunkArea];
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkSizeVector);

        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp, chunkArea);

        while (tiles.MoveNext(out var tile))
        {
            InitializeTemperatureAtTile(grid, tile.GridIndices, ref temperatureMap, ent.Comp.ChunkIndices);
        }

        ent.Comp.TemperatureMap = temperatureMap;
    }

    private void InitializeTemperatureAtTile(
        Entity<MapGridComponent> grid,
        Vector2i gridIndices,
        ref TileHeat[] temperatures,
        Vector2i chunkIndices)
    {
        float? heatCapacity = null;
        float? temperature = null;
        float? conductance = null;

        // Try to get the anchored wall entity on this tile
        var ents = MapSystem.GetAnchoredEntitiesEnumerator(grid.Owner, grid.Comp, gridIndices);
        while (ents.MoveNext(out var anchored))
        {
            if (!_temperatureQuery.TryComp(anchored, out var tempContainer)
                || !_materialQuery.TryComp(anchored, out var materialComp))
                continue;

            var material = _proto.Index(materialComp.Material);
            temperature = tempContainer.StartingTemperature;
            heatCapacity = material.SpecificHeatCapacity * SubGridTileVolume * material.Density;
            conductance = material.ThermalConductivity * SubGridWorldSize;
            break;
        }

        if (heatCapacity == null
            || temperature == null
            || conductance == null)
            return;

        var subTiles = GetAreaTileIndexesAtTile(chunkIndices, gridIndices, grid.Comp.TileSizeVector);
        foreach (var index in subTiles)
        {
            temperatures[index] = new TileHeat(heatCapacity.Value, temperature.Value, conductance.Value);
        }
    }

    private void OnChunkDeleted(Entity<SubGridChunkComponent> ent, ref EntityTerminatingEvent args)
    {
        if (TerminatingOrDeleted(ent.Comp.ParentGrid)
            || !SubGridQuery.TryComp(ent.Comp.ParentGrid, out var subGrid))
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

        if (!MapGridQuery.Resolve(grid.Owner, ref grid.Comp2))
            return;

        // Go through all tiles to make sure that grid is fully covered.
        var positions = new HashSet<Vector2i>();
        var tiles = MapSystem.GetAllTilesEnumerator(grid.Owner, grid.Comp2);
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

    private void EnsureSubGridChunk(Entity<SubGridComponent> grid, Vector2i gridIndices, out Entity<SubGridChunkComponent> chunk)
    {
        if (grid.Comp.ChunkEntities.TryGetValue(GetChunkIndices(gridIndices), out var chunkUid))
        {
            chunk = (chunkUid, ChunkQuery.Comp(chunkUid));
            return;
        }

        chunk = SpawnSubGridChunk(grid, gridIndices);
    }

    /// <summary>
    /// Spawns a subgrid chunk entity on a grid at a specified position, aligned to chunk indices.
    /// </summary>
    /// <param name="grid">The relative grid.</param>
    /// <param name="gridIndices">The grid indices that are inside the chunk.</param>
    /// <returns>The spawned subgrid chunk.</returns>
    private Entity<SubGridChunkComponent> SpawnSubGridChunk(Entity<SubGridComponent> grid, Vector2i gridIndices)
    {
        var pos = GetChunkPosition(gridIndices);
        var chunk = Spawn(null, new EntityCoordinates(grid, pos));
        var chunkComp = EnsureComp<SubGridChunkComponent>(chunk);

        // TODO this is shitcode
        chunkComp.ParentGrid = grid.Owner;
        chunkComp.ChunkIndices = GetChunkIndices(gridIndices);

        var ev = new SubGridChunkInitializeEvent();
        RaiseLocalEvent(chunk, ref ev);

        grid.Comp.ChunkEntities.Add(chunkComp.ChunkIndices, chunk);
        Log.Info($"Added chunk {ToPrettyString(chunk)} to grid {ToPrettyString(grid)} with chunkIndices {chunkComp.ChunkIndices}");
        return (chunk, chunkComp);
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
        if (!MapGridQuery.Resolve(grid.Owner, ref grid.Comp2))
            return false;

        // Check 10x10 box around the center of the chunk for any tiles that are not empty.
        var box = Box2.CenteredAround(GetChunkPosition(position),
            new Vector2(SystemConstants.PvsChunkSize + 2, SystemConstants.PvsChunkSize + 2));

        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, box);
        while (tiles.MoveNext(out _))
        {
            // Even a single iteration means that there are some tiles here, and the chunk should stay.
            return false;
        }

        var indices = GetChunkIndices(position);
        if (!grid.Comp1.ChunkEntities.TryGetValue(indices, out var chunk))
            return false; // It was already removed

        QueueDel(chunk);
        Log.Info($"Removing chunk {ToPrettyString(chunk)} at chunk indices {indices} on grid {ToPrettyString(grid)}");
        return true;
    }
}

[ByRefEvent]
public record struct SubGridChunkInitializeEvent();
