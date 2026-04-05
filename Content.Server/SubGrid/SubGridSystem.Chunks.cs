using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using Content.Shared.Maps;
using Content.Shared.Subgrid;
using Content.Shared.Subgrid.Chunks;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature;
using Content.Shared.Temperature.HeatContainers;
using Content.Shared.Utils;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

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

        SubscribeLocalEvent<SubGridComponent, SubGridInitializeEvent>(OnSubGridInit);
        SubscribeLocalEvent<SubGridChunkComponent, SubGridChunkInitializeEvent>(OnChunkInit);
        SubscribeLocalEvent<SubGridChunkComponent, EntityTerminatingEvent>(OnChunkDeleted);

        SubscribeLocalEvent<SubGridResizedEvent>(OnGridResized);
        SubscribeLocalEvent<SubGridHeightChangedEvent>(OnGridHeightChanged);
    }

    private void OnGridResized(ref SubGridResizedEvent ev)
    {
        var chunkQuery = EntityQueryEnumerator<SubGridChunkComponent>();
        while (chunkQuery.MoveNext(out var chunk, out var chunkComp))
        {
            foreach (var atmosTile in chunkComp.ChunkData.AtmosphereMap)
            {

            }

            foreach (var heatTile in chunkComp.ChunkData.TemperatureMap)
            {

            }
        }
    }

    private void OnGridHeightChanged(ref SubGridHeightChangedEvent ev)
    {

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

    private HashSet<EntityUid> _chunkInitializeCache = new(16);

    private void OnGridChanged(Entity<SubGridComponent> ent, ref TileChangedEvent args)
    {
        if (Paused(ent.Owner)) // We don't care about it until we are initialized
            return;

        // TODO this doesn't support grid splitting
        _chunkInitializeCache.Clear();
        foreach (var change in args.Changes)
        {
            if (change.OldTile.TypeId == 0)
            {
                // If space changed to something else, initialize subgrid tiles on a new place
                EnsureSubGridChunkWithNeighbours(ent, change.GridIndices, ref _chunkInitializeCache);
            }
            else if (change.NewTile.TypeId == 0)
            {
                // Otherwise check the chunks to be removed
                TryRemoveSubGridChunkWithNeighbours(ent, change.GridIndices);
            }
        }

        foreach (var uid in _chunkInitializeCache)
        {
            var ev = new SubGridChunkInitializeEvent();
            RaiseLocalEvent(uid, ref ev);
        }

        // We have to go through the loop of all changes again
        // since only here we can be sure that all chunks were placed.
        foreach (var change in args.Changes)
        {
            if (change.OldTile.TypeId == 0)
            {
                if (!TryGetChunk(ent.AsNullable(), change.GridIndices, out var chunk))
                    continue;

                InitializeAtmosAtTile(
                    (ent.Owner, ent.Comp, args.Entity.Comp),
                    change.GridIndices,
                    chunk.Value.Comp.ChunkIndices,
                    chunk.Value.Comp.ChunkData);

                AtmosNeighboursCache.Clear();
                ResolveMapRelativeToChunk(ent, ref AtmosNeighboursCache, chunk.Value.Comp.ChunkIndices);

                InitializeTileBorders((ent.Owner, ent.Comp, args.Entity.Comp), change.GridIndices, AtmosNeighboursCache);
                //ApplyMap(ent, ref AtmosNeighboursCache);
                Dirty(chunk.Value); // TODO ATL optimization
            }
            else if (change.NewTile.TypeId == 0)
            {
                // TODO subgrid cells removal
            }
        }

        Dirty(ent); // TODO ATL optimization
    }

    private void OnGridTerminating(Entity<SubGridComponent> ent, ref EntityTerminatingEvent args)
    {
        foreach (var (_, uid) in ent.Comp.ChunkEntities)
        {
            QueueDel(uid);
        }
    }

    private void OnSubGridInit(Entity<SubGridComponent> ent, ref SubGridInitializeEvent args)
    {
        foreach (var (_, chunk) in ent.Comp.ChunkEntities)
        {
            var comp = ChunkQuery.Comp(chunk);
            InitChunk((chunk, comp));
        }

        foreach (var (_, chunk) in ent.Comp.ChunkEntities)
        {
            var comp = ChunkQuery.Comp(chunk);
            InitializeChunkBorders((chunk, comp));
        }
    }

    private void OnChunkInit(Entity<SubGridChunkComponent> ent, ref SubGridChunkInitializeEvent args)
    {
        InitChunk(ent);
        InitializeChunkBorders(ent);
    }

    private void InitChunk(Entity<SubGridChunkComponent> ent)
    {
        if (!MapGridQuery.TryComp(ent.Comp.ParentGrid, out var mapGridComp)
            || !SubGridQuery.TryComp(ent.Comp.ParentGrid, out var subGridComp))
        {
            Log.Error("SubGrid chunk was initialized without a parent grid!");
            return;
        }

        InitializeChunkTemperature((ent.Owner, ent.Comp), (ent.Comp.ParentGrid, subGridComp, mapGridComp));
        InitializeChunkAtmos((ent.Owner, ent.Comp), (ent.Comp.ParentGrid, subGridComp, mapGridComp));
        Dirty(ent);
    }

    // TODO this is bad and also prevents multithreading
    public Dictionary<Vector2i, SubGridChunk> AtmosNeighboursCache = new(10);

    private void InitializeChunkAtmos(Entity<SubGridChunkComponent> ent, Entity<SubGridComponent, MapGridComponent> grid)
    {
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkSizeVector);
        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, chunkArea);

        while (tiles.MoveNext(out var tile))
        {
            var mixture = _atmos.GetGridMixture(grid.Owner);
            var isAirtight = false;
            var ents = MapSystem.GetAnchoredEntitiesEnumerator(grid.Owner, grid.Comp2, tile.GridIndices);
            while (ents.MoveNext(out var anchored))
            {
                if (_materialQuery.HasComp(anchored))
                    isAirtight = true;

                if (_markerQuery.TryComp(anchored, out var markerComp))
                    mixture = (GasMixture) _proto.Index(markerComp.Mixture).Definition.CreateMixture(
                        GasFactory,
                        _proto,
                        SubGridTileVolume);
            }

            if (isAirtight)
                continue;

            InitializeAtmosAtTile(grid, tile.GridIndices, ent.Comp.ChunkIndices, ent.Comp.ChunkData, mixture);
        }
    }

    private void InitializeAtmosAtTile(
        Entity<SubGridComponent, MapGridComponent> grid,
        Vector2i gridIndices,
        Vector2i chunkIndices,
        SubGridChunk chunk,
        GasMixture? mixture = null)
    {
        mixture ??= _atmos.GetSpaceTileMixture();
        var subTiles = GetAreaTileIndexesAtTile(chunkIndices, gridIndices, grid.Comp2.TileSizeVector);
        foreach (var index in subTiles)
        {
            chunk.AtmosphereMap[index] = new TileAtmos(mixture.Value);
        }
    }

    private void InitializeChunkBorders(Entity<SubGridChunkComponent> ent)
    {
        if (!MapGridQuery.TryComp(ent.Comp.ParentGrid, out var mapGridComp)
            || !SubGridQuery.TryComp(ent.Comp.ParentGrid, out var subGridComp))
        {
            Log.Error("SubGrid chunk was initialized without a parent grid!");
            return;
        }

        Entity<SubGridComponent, MapGridComponent> grid = (ent.Comp.ParentGrid, subGridComp, mapGridComp);
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkSizeVector + grid.Comp2.TileSizeVector * 2f);
        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, chunkArea);

        AtmosNeighboursCache.Clear();
        ResolveMapRelativeToChunk(grid, ref AtmosNeighboursCache, ent.Comp.ChunkIndices);
        while (tiles.MoveNext(out var tile))
        {
            InitializeTileBorders(grid, tile.GridIndices, AtmosNeighboursCache);
        }
    }

    private void InitializeTileBorders(
        Entity<SubGridComponent, MapGridComponent> grid,
        Vector2i gridIndices,
        Dictionary<Vector2i, SubGridChunk> nearChunks)
    {
        // Check if it is located near space, and add a boundary layer of atmosphere tiles.
        var spaceMix = _atmos.GetSpaceTileMixture();
        GasFactory.MarkImmutable(ref spaceMix);
        foreach (var vecDir in DirectionsWithDiagonals)
        {
            var tileRef = MapSystem.GetTileRef(grid.Owner, grid.Comp2, gridIndices + vecDir);
            var tileData = (ContentTileDefinition) _tileDefMan[tileRef.Tile.TypeId];
            if (!tileData.MapAtmosphere)
                continue;

            // Make a tile box with a layer of subgrid tiles and get all indexes to place the tiles at.
            var tileBox = new Box2(gridIndices, gridIndices + grid.Comp2.TileSizeVector).Enlarged(SubGridWorldSize);
            var found = GetTileIndexesWorld(tileBox);

            foreach (var (foundIndices, indexes) in found)
            {
                if (!nearChunks.TryGetValue(foundIndices, out var foundAtmosGridCorner))
                {
                    Log.Error($"When trying to initialize chunk for tile {gridIndices}, there was somehow no neighbouring empty chunk at {foundIndices}.");
                    continue;
                }

                foreach (var index in indexes)
                {
                    if (foundAtmosGridCorner.AtmosphereMap[index].Initialized
                        || foundAtmosGridCorner.TemperatureMap[index].Initialized)
                        continue;

                    foundAtmosGridCorner.AtmosphereMap[index] = new TileAtmos(spaceMix);
                }
            }

            // All available edge tiles were placed already,
            // so we don't have to check other directions.
            break;
        }
    }

    private void InitializeChunkTemperature(Entity<SubGridChunkComponent> ent, Entity<SubGridComponent, MapGridComponent> grid)
    {
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkSizeVector);

        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, chunkArea);
        while (tiles.MoveNext(out var tile))
        {
            InitializeTemperatureAtTile(grid, tile.GridIndices, ent.Comp.ChunkIndices, ent.Comp.ChunkData);
        }
    }

    private void InitializeTemperatureAtTile(
        Entity<SubGridComponent, MapGridComponent> grid,
        Vector2i gridIndices,
        Vector2i chunkIndices,
        SubGridChunk chunk)
    {
        float? heatCapacity = null;
        float? temperature = null;
        float? conductance = null;

        // Try to get the anchored wall entity on this tile
        var ents = MapSystem.GetAnchoredEntitiesEnumerator(grid.Owner, grid.Comp2, gridIndices);
        while (ents.MoveNext(out var anchored))
        {
            if (!_temperatureQuery.TryComp(anchored, out var tempContainer)
                || !_materialQuery.TryComp(anchored, out var materialComp))
                continue;

            var material = _proto.Index(materialComp.Material);
            temperature = tempContainer.StartingTemperature;
            // Volume * Density = Mass, SpecificHeatCapacity * Mass = HeatCapacity.
            heatCapacity = material.SpecificHeatCapacity * SubGridTileVolume * material.Density;
            // Conductance = ThermalConductivity * Characteristic Length.
            // Height is taken because of how Fourier's Law is canceled out for rectangular prisms.
            conductance = material.ThermalConductivity * SubGridHeight;
            break;
        }

        if (heatCapacity == null
            || temperature == null
            || conductance == null)
            return;

        var subTiles = GetAreaTileIndexesAtTile(chunkIndices, gridIndices, grid.Comp2.TileSizeVector);
        foreach (var index in subTiles)
        {
            chunk.TemperatureMap[index] = new TileHeat(heatCapacity.Value, temperature.Value, conductance.Value);
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

        _chunkInitializeCache.Clear();
        foreach (var pos in positions)
        {
            EnsureSubGridChunkWithNeighbours(grid, pos, ref _chunkInitializeCache);
        }

        var ev = new SubGridInitializeEvent();
        RaiseLocalEvent(grid.Owner, ref ev);

        Log.Info($"Successfully initialized SubGrid on grid {ToPrettyString(grid.Owner)}. Total amount of chunks: {grid.Comp1.ChunkEntities.Count}");
    }

    /// <summary>
    /// Ensures that a given tile position and also all neighbours in 8 directions
    /// have an assigned subgrid chunk.
    /// </summary>
    private void EnsureSubGridChunkWithNeighbours(Entity<SubGridComponent> grid, Vector2i gridIndices, ref HashSet<EntityUid> set)
    {
        if (TryEnsureSubGridChunk(grid, gridIndices, out var spawned))
            set.Add(spawned.Value);

        foreach (var dir in DirectionsWithDiagonals)
        {
            if (TryEnsureSubGridChunk(grid, gridIndices + dir, out var spawnedDir))
                set.Add(spawnedDir.Value);
        }
    }

    /// <summary>
    /// Ensures that a given tile position has an assigned subgrid chunk.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="gridIndices"></param>
    private bool TryEnsureSubGridChunk(Entity<SubGridComponent> grid, Vector2i gridIndices, [NotNullWhen(true)] out EntityUid? spawned)
    {
        spawned = null;
        if (grid.Comp.ChunkEntities.ContainsKey(GetChunkIndicesTile(gridIndices)))
            return false;

        spawned = SpawnSubGridChunk(grid, gridIndices);
        return true;
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
        if (grid.Comp.ChunkEntities.TryGetValue(GetChunkIndicesTile(gridIndices), out chunk))
            return;

        chunk = SpawnSubGridChunk(grid, gridIndices);
    }

    private void EnsureSubGridChunk(Entity<SubGridComponent> grid, Vector2i gridIndices, out Entity<SubGridChunkComponent> chunk)
    {
        if (grid.Comp.ChunkEntities.TryGetValue(GetChunkIndicesTile(gridIndices), out var chunkUid))
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
        chunkComp.ChunkIndices = GetChunkIndicesTile(gridIndices);
        chunkComp.ChunkData = new SubGridChunk(SubGridChunkSize);
        chunkComp.AtmosBuffer = new GasMixture[SubGridChunkArea];
        chunkComp.HeatBuffer = new ConductiveHeatContainer[SubGridChunkArea];
        chunkComp.ChunkBuffer = new Dictionary<Vector2i, SubGridChunk>(9);
        chunkComp.GasArrayPool = new ConstantArrayPool<float>(GasFactory.ArraySize, GasMixtureFactory.BufferBucketSize);

        grid.Comp.ChunkEntities.Add(chunkComp.ChunkIndices, chunk);

        Log.Info($"Added chunk {ToPrettyString(chunk)} to grid {ToPrettyString(grid)} with chunkIndices {chunkComp.ChunkIndices}");
        Dirty(grid);
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
    private bool TryRemoveSubGridChunk(Entity<SubGridComponent, MapGridComponent?> grid, Vector2i gridIndices)
    {
        if (!MapGridQuery.Resolve(grid.Owner, ref grid.Comp2))
            return false;

        // Check 10x10 box around the center of the chunk for any tiles that are not empty.
        var box = Box2.CenteredAround(GetChunkPosition(gridIndices),
            new Vector2(SystemConstants.PvsChunkSize + 2, SystemConstants.PvsChunkSize + 2));

        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, box);
        while (tiles.MoveNext(out _))
        {
            // Even a single iteration means that there are some tiles here, and the chunk should stay.
            return false;
        }

        var indices = GetChunkIndicesTile(gridIndices);
        if (!grid.Comp1.ChunkEntities.TryGetValue(indices, out var chunk))
            return false; // It was already removed

        QueueDel(chunk);
        Log.Info($"Removing chunk {ToPrettyString(chunk)} at chunk indices {indices} on grid {ToPrettyString(grid)}");
        return true;
    }
}

/// <summary>
/// Raised on a subgrid that already set up all chunks and now is initializing.
/// </summary>
[ByRefEvent]
public record struct SubGridInitializeEvent();

/// <summary>
/// Raised on a chunk explicitly to tell it to initialize.
/// </summary>
[ByRefEvent]
public record struct SubGridChunkInitializeEvent();
