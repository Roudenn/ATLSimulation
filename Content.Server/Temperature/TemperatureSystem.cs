using Content.Server.SubGrid;
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

    public override void Initialize()
    {
        base.Initialize();
        _subgridChunkQuery = GetEntityQuery<SubGridChunkComponent>();
    }

    private Dictionary<Vector2i, TileHeat[]> _heatCache = new(64);
    private TileHeat[] _chunkCache = new TileHeat[0];
    private TimeSpan _lastUpdate = TimeSpan.Zero;

    public void UpdateHeat()
    {
        if (!TemperatureEnabled)
            return;

        // TODO make this an event
        Array.Resize(ref _chunkCache, _subGrid.SubGridChunkArea);

        var query = EntityQueryEnumerator<SubGridComponent>();
        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
        deltaTime = MathF.Min(deltaTime, 0.5f);
        _lastUpdate = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            _heatCache.Clear();
            _subGrid.ResolveHeatMap((uid, comp), ref _heatCache);

            foreach (var (chunkIndices, chunkData) in _heatCache)
            {
                var chunkEnt = comp.ChunkEntities[chunkIndices];
                if (!_subgridChunkQuery.TryComp(chunkEnt, out var chunkComp))
                    continue;

                for (var i = 0; i < chunkData.Length; i++)
                {
                    var tile = chunkData[i];
                    if (!tile.Initialized)
                        continue;

                    _chunkCache[i] = ProcessTile(tile, chunkIndices, i, deltaTime);
                }

                chunkComp.TemperatureMap = _chunkCache;
            }

            Dirty(uid, comp);
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
            var alteredTime = deltaTime;
            if (!SharedSubGridSystem.Directions.Contains(dir))
                alteredTime /= 2f;

            var foundTile = found.Value;
            TileHeatHelpers.ConductHeatTiles(ref newTile, ref foundTile, alteredTime);
        }

        return newTile;
    }
}
