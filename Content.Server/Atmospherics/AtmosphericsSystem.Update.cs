using Content.Shared.Atmospherics;
using Content.Shared.Subgrid;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem
{
    private Dictionary<Vector2i, SubGridChunk> _atmosCache = new(64);
    private TileAtmos[] _chunkCache = new TileAtmos[0];
    private TimeSpan _lastUpdate = TimeSpan.Zero;

    public void UpdateAtmos()
    {
        if (!AtmosEnabled)
            return;

        var query = EntityQueryEnumerator<SubGridComponent>();
        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
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

                for (var i = 0; i < chunkData.AtmosphereMap.Length; i++)
                {
                    var tile = chunkData.AtmosphereMap[i];
                    if (!tile.Initialized)
                        continue;

                    _chunkCache[i] = ProcessTile(tile, chunkIndices, i, deltaTime);
                }

                chunkComp.ChunkData.AtmosphereMap = _chunkCache;
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
            GasManager.DiffuseMixturesArchived(
                ref newTile.ArchivedMixture,
                ref foundTile.ArchivedMixture,
                ref newTile.Mixture,
                ref foundTile.Mixture,
                _subGrid.SubGridWorldSize,
                alteredTime);
        }

        return newTile;
    }
}
