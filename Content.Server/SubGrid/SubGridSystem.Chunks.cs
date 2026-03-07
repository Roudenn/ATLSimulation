using System.Numerics;
using Content.Shared.Atmospherics;
using Content.Shared.Constants;
using Content.Shared.Maps;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature;
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
        if (Paused(ent.Owner)) // We don't care about it until we are initialized
            return;

        // TODO this doesn't support grid splitting
        foreach (var change in args.Changes)
        {
            if (change.OldTile.TypeId == 0)
            {
                // If space changed to something else, initialize subgrid tiles on a new place
                EnsureSubGridChunkWithNeighbours(ent, change.GridIndices);
            }
            else if (change.NewTile.TypeId == 0)
            {
                // Otherwise check the chunks to be removed
                TryRemoveSubGridChunkWithNeighbours(ent, change.GridIndices);
                // TODO subgrid cells removal
            }
        }

        // We have to go through the loop of all changes again
        // since only here we can be sure that all chunks were placed.
        foreach (var change in args.Changes)
        {
            if (change.OldTile.TypeId == 0)
            {
                if (!TryGetChunk(ent.AsNullable(), change.GridIndices, out var chunk))
                    continue;

                var ev = new SubGridChunkInitializeEvent();
                RaiseLocalEvent(chunk.Value.Owner, ref ev);
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
    }

    private void OnChunkInit(Entity<SubGridChunkComponent> ent, ref SubGridChunkInitializeEvent args)
    {
        InitChunk(ent);
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
    public Dictionary<Vector2i, TileAtmos[]> AtmosNeighboursCache = new(10);

    private void InitializeChunkAtmos(Entity<SubGridChunkComponent> ent, Entity<SubGridComponent, MapGridComponent> grid)
    {
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkSizeVector);
        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, chunkArea);

        AtmosNeighboursCache.Clear();
        ResolveAtmosMapRelativeToChunk(grid, ref AtmosNeighboursCache, ent.Comp.ChunkIndices);
        while (tiles.MoveNext(out var tile))
        {
            var airtight = false;
            var ents = MapSystem.GetAnchoredEntitiesEnumerator(grid.Owner, grid.Comp2, tile.GridIndices);
            while (ents.MoveNext(out var anchored))
            {
                if (!_materialQuery.HasComp(anchored))
                    continue;

                airtight = true;
                break;
            }

            if (airtight)
                continue;

            InitializeAtmosAtTile(grid, tile.GridIndices, ent.Comp.ChunkIndices, ref ent.Comp.AtmosphereMap, AtmosNeighboursCache, true);
        }
    }

    private void InitializeAtmosAtTile(
        Entity<SubGridComponent, MapGridComponent> grid,
        Vector2i gridIndices,
        Vector2i chunkIndices,
        ref TileAtmos[] atmos,
        Dictionary<Vector2i, TileAtmos[]> nearChunks,
        bool gridMixture = false)
    {
        // TODO add airtight stuff
        var spaceMix = _atmos.GetSpaceMixture();
        var gridMix = gridMixture ? _atmos.GetGridMixture(grid.Owner) : spaceMix;

        var subTiles = GetAreaTileIndexesAtTile(chunkIndices, gridIndices, grid.Comp2.TileSizeVector);
        foreach (var index in subTiles)
        {
            atmos[index] = new TileAtmos(gridMix);
        }

        // At last, the tile has to check if it is located near space, and add a boundary layer of atmosphere tiles.
        foreach (var vecDir in DirectionsWithDiagonals)
        {
            var tileRef = MapSystem.GetTileRef(grid.Owner, grid.Comp2, gridIndices + vecDir);
            var tileData = (ContentTileDefinition) _tileDefMan[tileRef.Tile.TypeId];
            if (!tileData.MapAtmosphere)
                continue;

            var dir = vecDir.GetDir();
            var indexCorners = GetTileCornerIndexes(chunkIndices, gridIndices, grid.Comp2.TileSizeVector);

            // Handle corners
            var corner = BoxIndexAtCornerDirection(indexCorners, dir);
            if (corner != null)
            {
                var (foundChunk, index) = GetTileRelative(chunkIndices, VectorToIndex(corner.Value), vecDir);
                if (!nearChunks.TryGetValue(foundChunk, out var foundAtmosGridCorner))
                {
                    Log.Error($"When trying to initialize chunk at {chunkIndices} for tile {gridIndices}, there was somehow no neighbouring empty chunk at {foundChunk}.");
                    continue;
                }

                foundAtmosGridCorner[index] = new TileAtmos(spaceMix);
                continue;
            }

            // Handle sides
            var moveVec = vecDir.Rotate(Angle.FromDegrees(90));
            var startIndex = BoxIndexAtDirection(indexCorners, dir);

            // Since the chunk doesn't change we have to check if it's right only once.
            var (chunkNeighbour, start) = GetTileRelative(chunkIndices, VectorToIndex(startIndex), vecDir);
            if (!nearChunks.TryGetValue(chunkNeighbour, out var foundAtmosGrid))
            {
                Log.Error($"When trying to initialize chunk at {chunkIndices}, there was somehow no neighbouring empty chunk.");
                continue;
            }

            for (int i = 0; i < SubGridTileSize - 1; i++)
            {
                var currentIndex = start + VectorToIndex(moveVec * i);
                var (_, index) = GetTileRelative(chunkIndices, currentIndex, vecDir);
                foundAtmosGrid[index] = new TileAtmos(spaceMix);
            }
        }
    }

    public static Vector2i? BoxIndexAtCornerDirection(Box2i box, Direction dir)
    {
        return dir switch
        {
            Direction.NorthWest => box.TopLeft,
            Direction.NorthEast => box.TopRight,
            Direction.SouthWest => box.BottomLeft,
            Direction.SouthEast => box.BottomRight,
            _ => null,
        };
    }

    public static Vector2i BoxIndexAtDirection(Box2i box, Direction dir)
    {
        return dir switch
        {
            Direction.North => box.TopLeft,
            Direction.East => box.TopRight,
            Direction.West => box.BottomLeft,
            Direction.South => box.BottomRight,
            _ => throw new ArgumentException(),
        };
    }

    private void InitializeChunkTemperature(Entity<SubGridChunkComponent> ent, Entity<SubGridComponent, MapGridComponent> grid)
    {
        var chunkPos = ChunkIndicesToPosition(ent.Comp.ChunkIndices);
        var chunkArea = Box2.CenteredAround(chunkPos, ChunkSizeVector);

        var tiles = MapSystem.GetLocalTilesEnumerator(grid.Owner, grid.Comp2, chunkArea);

        while (tiles.MoveNext(out var tile))
        {
            InitializeTemperatureAtTile(grid, tile.GridIndices, ref ent.Comp.TemperatureMap, ent.Comp.ChunkIndices);
        }
    }

    private void InitializeTemperatureAtTile(
        Entity<SubGridComponent, MapGridComponent> grid,
        Vector2i gridIndices,
        ref TileHeat[] temperatures,
        Vector2i chunkIndices)
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
            heatCapacity = material.SpecificHeatCapacity * SubGridTileVolume * material.Density;
            conductance = material.ThermalConductivity * SubGridWorldSize;
            break;
        }

        if (heatCapacity == null
            || temperature == null
            || conductance == null)
            return;

        var subTiles = GetAreaTileIndexesAtTile(chunkIndices, gridIndices, grid.Comp2.TileSizeVector);
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

        var ev = new SubGridInitializeEvent();
        RaiseLocalEvent(grid.Owner, ref ev);

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

        // preallocate the memory
        chunkComp.AtmosphereMap = new TileAtmos[SubGridChunkArea];
        chunkComp.TemperatureMap = new TileHeat[SubGridChunkArea];

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
