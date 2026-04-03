using System.Linq;
using Content.Shared.Atmospherics;
using Content.Shared.Subgrid;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Utils;
using Robust.Shared.Threading;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem
{
    private TimeSpan _lastUpdate = TimeSpan.Zero;

    private ProcessAtmosDiffusion _diffusionJob;

    private void InitializeUpdate()
    {
        _diffusionJob = new(_subGrid, this);
    }

    public void UpdateAtmos()
    {
        if (!AtmosEnabled)
            return;

        var query = EntityQueryEnumerator<SubGridComponent>();
        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
        deltaTime = MathF.Min(deltaTime, 0.01f);
        _lastUpdate = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            var indicies = comp.ChunkEntities.Keys.ToList();

            _diffusionJob.SubGrid = (uid, comp);
            _diffusionJob.ChunkIndices = indicies;
            _diffusionJob.DeltaTime = deltaTime / AtmosSteps;
            for (int i = 0; i < AtmosSteps; i++)
            {
                _parallel.ProcessNow(_diffusionJob, indicies.Count);

                foreach (var chunkIndices in indicies)
                {
                    var chunkEnt = comp.ChunkEntities[chunkIndices];
                    if (!_subgridChunkQuery.TryComp(chunkEnt, out var chunkComp))
                        continue;

                    for (var index = 0; index < chunkComp.ChunkData.TemperatureMap.Length; index++)
                    {
                        chunkComp.ChunkData.AtmosphereMap[index].ArchivedMixture = chunkComp.ChunkData.AtmosphereMap[index].Mixture;
                    }
                }
            }
        }
    }

    private record struct ProcessAtmosDiffusion(SharedSubGridSystem SubGridSystem, AtmosphericsSystem AtmosphericsSystem) : IParallelRobustJob
    {
        public int BatchSize => 1;

        public readonly SharedSubGridSystem SubGridSystem = SubGridSystem;

        public readonly AtmosphericsSystem AtmosphericsSystem = AtmosphericsSystem;

        public Entity<SubGridComponent> SubGrid;

        public List<Vector2i> ChunkIndices = new();

        public float DeltaTime;

        public void Execute(int index)
        {
            var indices = ChunkIndices[index];
            var chunkBuffer = SubGrid.Comp.ChunkMapCaches[indices];
            var chunkGasBuffer = SubGrid.Comp.ChunkGasBuffers[indices];
            chunkBuffer.Clear();
            SubGridSystem.ResolveMapRelativeToChunk(SubGrid, ref chunkBuffer, indices);
            AtmosphericsSystem.ProcessChunk(chunkBuffer, indices, DeltaTime, chunkGasBuffer);
        }
    }

    public void ProcessChunk(
        Dictionary<Vector2i, SubGridChunk> chunkBuffer,
        Vector2i indices,
        float deltaTime,
        IRobustArrayPool<float> pool)
    {
        var chunkData = chunkBuffer[indices];
        for (var i = 0; i < chunkData.AtmosphereMap.Length; i++)
        {
            var tile = chunkData.AtmosphereMap[i];
            if (!tile.Initialized)
                continue;

            chunkData.AtmosphereMap[i] = ProcessTile(tile, i, chunkBuffer, indices, deltaTime, pool);
        }
    }

    private TileAtmos ProcessTile(
        TileAtmos tile,
        int index,
        Dictionary<Vector2i, SubGridChunk> chunkBuffer,
        Vector2i chunkIndices,
        float deltaTime,
        IRobustArrayPool<float> pool)
    {
        foreach (var dir in SharedSubGridSystem.DirectionsWithDiagonals)
        {
            if (!_subGrid.TryGetAtmosphereTileRelative(chunkBuffer, chunkIndices, index, dir, out var found))
                continue;

            // The coefficients for interaction get halved when going diagonally,
            // since Characteristic Length is multiplied by √2 and surface area is multiplied by √2/2.
            var alteredTime = deltaTime * AtmosSpeedup;
            if (!SharedSubGridSystem.Directions.Contains(dir))
            {
                alteredTime /= 2f;

                // Diagonal movement is only possible if there are also neighbouring tiles.
                if (!_subGrid.TryGetAtmosphereTileRelative(chunkBuffer, chunkIndices, index, new Vector2i(dir.X, 0), out var foundFirst)
                    || !_subGrid.TryGetAtmosphereTileRelative(chunkBuffer, chunkIndices, index, new Vector2i(0, dir.Y), out var foundSecond)
                    || !foundFirst.Value.Initialized
                    || !foundSecond.Value.Initialized)
                    continue;
            }

            GasManager.DiffuseTiles(
                ref tile,
                found.Value,
                _subGrid.SubGridWorldSize,
                alteredTime,
                pool);
        }

        return tile;
    }
}
