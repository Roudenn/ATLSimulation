using Content.Server.SubGrid;
using Content.Shared.Subgrid;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Systems;
using Robust.Shared.Timing;

namespace Content.Server.Temperature;

public sealed class TemperatureSystem : SharedTemperatureSystem
{
    [Dependency] private readonly SubGridSystem _subGrid = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<SubGridChunkComponent> _subgridChunkQuery;

    private Dictionary<Vector2i, SubGridChunk> _heatCache = new(64);
    private TimeSpan _lastUpdate = TimeSpan.Zero;
    private TileHeat[] _cache = new TileHeat[0];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubGridResizedEvent>(OnResize);

        _subgridChunkQuery = GetEntityQuery<SubGridChunkComponent>();
    }

    private void OnResize(ref SubGridResizedEvent ev)
    {
        Array.Resize(ref _cache, _subGrid.SubGridChunkArea);
    }

    public void UpdateHeat()
    {
        if (!TemperatureEnabled)
            return;

        Array.Resize(ref _cache, _subGrid.SubGridChunkArea); // TODO nuke this
        var query = EntityQueryEnumerator<SubGridComponent>();
        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
        deltaTime = MathF.Min(deltaTime, 0.01f);
        _lastUpdate = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            _heatCache.Clear();
            _subGrid.ResolveMap((uid, comp), ref _heatCache);

            foreach (var (chunkIndices, chunkData) in _heatCache)
            {
                var chunkEnt = comp.ChunkEntities[chunkIndices];
                if (!_subgridChunkQuery.TryComp(chunkEnt, out var chunkComp))
                    continue;

                // Marking all previous tiles as not initialized is much faster than allocating a new array.
                for (int i = 0; i < _cache.Length; i++)
                {
                    _cache[i].Initialized = false;
                }

                for (var i = 0; i < chunkData.TemperatureMap.Length; i++)
                {
                    var tile = chunkData.TemperatureMap[i];
                    if (!tile.Initialized)
                        continue;

                    _cache[i] = ProcessTile(tile, chunkIndices, i, deltaTime);
                }

                // Write the values manually since with an equal sign it copies the reference to a cache instead of the cache itself.
                for (int i = 0; i < chunkComp.ChunkData.TemperatureMap.Length; i++)
                {
                    chunkComp.ChunkData.TemperatureMap[i] = _cache[i];
                }
            }

            foreach (var (chunkIndices, _) in _heatCache)
            {
                var chunkEnt = comp.ChunkEntities[chunkIndices];
                if (!_subgridChunkQuery.TryComp(chunkEnt, out var chunkComp))
                    continue;

                for (var index = 0; index < chunkComp.ChunkData.TemperatureMap.Length; index++)
                {
                    chunkComp.ChunkData.TemperatureMap[index].ArchivedContainer = chunkComp.ChunkData.TemperatureMap[index].Container;
                }
                Dirty(chunkEnt, chunkComp);
            }
        }
    }

    private TileHeat ProcessTile(TileHeat tile, Vector2i chunkIndices, int index, float deltaTime)
    {
        var newTile = new TileHeat(tile);
        foreach (var dir in SharedSubGridSystem.DirectionsWithDiagonals)
        {
            if (!_subGrid.TryGetHeatTileRelative(_heatCache, chunkIndices, index, dir, out var found))
                continue;

            // The coefficients for interaction get halved when going diagonally,
            // since Characteristic Length is multiplied by √2 and surface area is multiplied by √2/2.
            var alteredTime = deltaTime * TemperatureSpeedup;
            if (!SharedSubGridSystem.Directions.Contains(dir))
                alteredTime /= 2f;

            var foundTile = found.Value;
            TileHeatHelpers.ConductHeatTiles(ref newTile, ref foundTile, alteredTime);
        }

        return newTile;
    }
}
