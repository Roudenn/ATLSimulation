using Content.Shared.Atmospherics;
using Content.Shared.Subgrid;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Robust.Shared.Timing;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem
{
    private Dictionary<Vector2i, SubGridChunk> _atmosCache = new(64);
    private TileAtmos[] _cache = new TileAtmos[0];
    private TimeSpan _lastUpdate = TimeSpan.Zero;

    public void UpdateAtmos()
    {
        if (!AtmosEnabled)
            return;

        var sw = RStopwatch.StartNew();
        Array.Resize(ref _cache, _subGrid.SubGridChunkArea); // TODO nuke this
        var query = EntityQueryEnumerator<SubGridComponent>();
        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
        deltaTime = MathF.Min(deltaTime, 0.01f);
        _lastUpdate = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            _atmosCache.Clear();
            _subGrid.ResolveMap((uid, comp), ref _atmosCache);

            foreach (var (chunkIndices, chunkData) in _atmosCache)
            {
                var chunkEnt = comp.ChunkEntities[chunkIndices];
                if (!_subgridChunkQuery.TryComp(chunkEnt, out var chunkComp))
                    continue;

                // Marking all previous tiles as not initialized is much faster than allocating a new array.
                for (int i = 0; i < _cache.Length; i++)
                {
                    _cache[i].Initialized = false;
                }

                for (var i = 0; i < chunkData.AtmosphereMap.Length; i++)
                {
                    var tile = chunkData.AtmosphereMap[i];
                    if (!tile.Initialized)
                        continue;

                    _cache[i] = ProcessTile(tile, chunkIndices, i, deltaTime);
                }

                // Write the values manually since with an equal sign it
                // copies the reference to a cache instead of the cache itself.
                for (int i = 0; i < chunkComp.ChunkData.AtmosphereMap.Length; i++)
                {
                    chunkComp.ChunkData.AtmosphereMap[i] = _cache[i];
                }
            }

            Log.Info($"Atmosphere processing done in: {sw.Elapsed}");

            foreach (var (chunkIndices, _) in _atmosCache)
            {
                var chunkEnt = comp.ChunkEntities[chunkIndices];
                if (!_subgridChunkQuery.TryComp(chunkEnt, out var chunkComp))
                    continue;

                for (var index = 0; index < chunkComp.ChunkData.TemperatureMap.Length; index++)
                {
                    chunkComp.ChunkData.AtmosphereMap[index].ArchivedMixture = chunkComp.ChunkData.AtmosphereMap[index].Mixture;
                }
                Dirty(chunkEnt, chunkComp);
            }
        }
    }

    private TileAtmos ProcessTile(TileAtmos tile, Vector2i chunkIndices, int index, float deltaTime)
    {
        var newTile = new TileAtmos(tile);
        foreach (var dir in SharedSubGridSystem.DirectionsWithDiagonals)
        {
            if (!_subGrid.TryGetAtmosphereTileRelative(_atmosCache, chunkIndices, index, dir, out var found))
                continue;

            // The coefficients for interaction get halved when going diagonally,
            // since Characteristic Length is multiplied by √2 and surface area is multiplied by √2/2.
            var alteredTime = deltaTime;
            if (!SharedSubGridSystem.Directions.Contains(dir))
                alteredTime /= 2f;

            var foundTile = found.Value;
            GasManager.DiffuseTiles(
                ref newTile,
                ref foundTile,
                _subGrid.SubGridWorldSize,
                alteredTime);
        }

        return newTile;
    }
}
