using System.Numerics;
using Content.Server.Atmospherics;
using Content.Shared.Atmospherics;
using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.SubGrid;

public sealed partial class SubGridSystem : SharedSubGridSystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly AtmosphericsSystem _atmos = default!;

    private EntityQuery<SubGridComponent> _subGridQuery;
    private EntityQuery<MapGridComponent> _mapGridQuery;
    
    private Dictionary<Vector2i, TileAtmosphere[]> AtmosphereCache = new(256);
    private Dictionary<Vector2i, TileTemperature[]> TemperatureCache = new(256);
    
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<GridInitializeEvent>(OnGridInit);
        SubscribeLocalEvent<SubGridChunkComponent, MapInitEvent>(OnChunkInit);
        SubscribeLocalEvent<SubGridChunkComponent, EntityTerminatingEvent>(OnChunkDeleted);
        
        _subGridQuery = GetEntityQuery<SubGridComponent>();
        _mapGridQuery = GetEntityQuery<MapGridComponent>();
    }
    
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SubGridComponent>();
        while (query.MoveNext(out var uid, out var grid))
        {
            // Set up proper maps for work
            AtmosphereCache.Clear();
            TemperatureCache.Clear();
            ResolveAtmosphereChunkMap((uid, grid), ref AtmosphereCache);
            ResolveTemperatureChunkMap((uid, grid), ref TemperatureCache);
            
            // TODO add tick rate scaling
            //_atmos.ProcessAtmosGrid((uid, grid), frameTime);
        }
    }
    
    public static Vector2i GetChunkIndices(Vector2 coordinates) => (coordinates / SystemConstants.PvsChunkSize).Floored();

    private void OnGridInit(GridInitializeEvent ev)
    {
        if (HasComp<MapComponent>(ev.EntityUid))
            return;

        EnsureComp<SubGridComponent>(ev.EntityUid);
    }

    private void OnChunkInit(Entity<SubGridChunkComponent> ent, ref MapInitEvent args)
    {
        var xform = Transform(ent.Owner);
        if (!_mapGridQuery.TryComp(xform.GridUid, out var mapGridComp))
            return;
        
        var atmos = new TileAtmosphere[SubGridSize * SubGridSize * SystemConstants.PvsChunkSize * SystemConstants.PvsChunkSize];

        var chunkArea = Box2.CenteredAround(xform.LocalPosition, new Vector2(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize));
        var tiles = _mapSystem.GetLocalTilesEnumerator(xform.GridUid.Value, mapGridComp, chunkArea);
        
    }

    private void OnChunkDeleted(Entity<SubGridChunkComponent> ent, ref EntityTerminatingEvent args)
    {
        var xform = Transform(ent);
        if (!_subGridQuery.TryComp(xform.ParentUid, out var subGrid))
            return;
        
        subGrid.ChunkEntities.Remove((Vector2i) xform.LocalPosition);
    }
    
    /// <summary>
    /// Ensures that every single tile of the mapgrid is covered by the subgrid.
    /// </summary>
    /// <remarks>
    /// This may cause huge lag spikes when called in the simulation runtime!
    /// If there's a known position for an update, use <see cref=""/> instead.
    /// </remarks>
    /// <param name="grid">The target grid to ensure all tiles on.</param>
    private void EnsureSubGrid(Entity<MapGridComponent, SubGridComponent> grid)
    {
        // TODO: this is laggy, would be better to make larger tile steps.
        // This method shouldn't be called on runtime for now, so i guess that's fine.
        
        // Go through all tiles to make sure that grid is fully covered.
        var positions = new HashSet<Vector2i>();
        var tiles = _mapSystem.GetAllTilesEnumerator(grid.Owner, grid.Comp1); 
        while (tiles.MoveNext(out var tile))
        {
            positions.Add(tile.Value.GridIndices);
        }

        foreach (var pos in positions)
        {
            if (grid.Comp2.ChunkEntities.ContainsKey(pos))
                continue; // Chunk is already placed here

            var chunk = SpawnChunk(grid.Owner, pos);
            grid.Comp2.ChunkEntities.Add(pos, chunk);
        }
    }

    /// <summary>
    /// Ensures that a given tile position has an assigned subgrid chunk.
    /// Returns either a new chunk or an already existing one that contains that tile.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="gridIndices"></param>
    /// <param name="chunk"></param>
    private void EnsureSubGridChunk(Entity<MapGridComponent, SubGridComponent> grid, Vector2i gridIndices, out EntityUid chunk)
    {
        if (grid.Comp2.ChunkEntities.TryGetValue(gridIndices, out chunk))
            return; // Chunk is already placed here, return it

        chunk = SpawnChunk(grid.Owner, gridIndices);
        grid.Comp2.ChunkEntities.Add(gridIndices, chunk);
    }
    
    /// <summary>
    /// Spawns a subgrid chunk entity on a grid at a specified position, aligned to chunk indices.
    /// </summary>
    /// <param name="grid">The relative grid.</param>
    /// <param name="gridIndices">The grid indices that are inside the chunk.</param>
    /// <returns>The spawned subgrid chunk.</returns>
    private EntityUid SpawnChunk(EntityUid grid, Vector2i gridIndices)
    {
        var pos = GetChunkIndices(gridIndices);
        var chunk = Spawn(null, new EntityCoordinates(grid, pos));
        // Sometimes they will be placed on empty tiles, but they still should be a part of the grid regardless
        _xform.SetParent(chunk, grid);
        EnsureComp<SubGridChunkComponent>(chunk);
        return chunk;
    }
}
