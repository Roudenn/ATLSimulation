using Content.Shared.Atmospherics;
using Content.Shared.Subgrid.Chunks;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Utils;
using Robust.Shared.Collections;
using Robust.Shared.Threading;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem
{
    private TimeSpan _lastUpdate = TimeSpan.Zero;

    private ProcessSimpleAtmosMovement _simpleMovementJob;
    private ProcessAtmosDiffusion _diffusionJob;

    private void InitializeUpdate()
    {
        _diffusionJob = new(_subGrid, this);
        _simpleMovementJob = new(_subGrid, this);
    }

    public void UpdateAtmos()
    {
        var query = EntityQueryEnumerator<SubGridComponent>();
        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
        deltaTime = MathF.Min(deltaTime, 0.05f);
        _lastUpdate = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            var chunks = new ValueList<Entity<SubGridChunkComponent>>(comp.ChunkEntities.Count);
            foreach (var ent in comp.ChunkEntities.Values)
            {
                if (!_subgridChunkQuery.TryComp(ent, out var chunkComp))
                    continue;

                chunks.Add((ent, chunkComp));
            }

            if (AtmosMovementEnabled)
            {
                _simpleMovementJob.SubGrid = (uid, comp);
                _simpleMovementJob.Chunks = chunks;
                _simpleMovementJob.DeltaTime = deltaTime / AtmosSteps;

                for (int i = 0; i < AtmosSteps; i++)
                {
                    _parallel.ProcessNow(_simpleMovementJob, comp.ChunkEntities.Count);

                    foreach (var chunk in chunks)
                    {
                        for (var index = 0; index < chunk.Comp.ChunkData.TemperatureMap.Length; index++)
                        {
                            chunk.Comp.ChunkData.AtmosphereMap[index].Mixture =
                                chunk.Comp.ChunkData.AtmosphereMap[index].CachedMixture;
                        }
                    }
                }
            }

            if (AtmosDiffusionEnabled)
            {
                _diffusionJob.SubGrid = (uid, comp);
                _diffusionJob.Chunks = chunks;
                _diffusionJob.DeltaTime = deltaTime / AtmosSteps;

                for (int i = 0; i < AtmosSteps; i++)
                {
                    _parallel.ProcessNow(_diffusionJob, comp.ChunkEntities.Count);

                    foreach (var chunk in chunks)
                    {
                        for (var index = 0; index < chunk.Comp.ChunkData.TemperatureMap.Length; index++)
                        {
                            chunk.Comp.ChunkData.AtmosphereMap[index].Mixture =
                                chunk.Comp.ChunkData.AtmosphereMap[index].CachedMixture;
                        }
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

        public ValueList<Entity<SubGridChunkComponent>> Chunks = new();

        public float DeltaTime;

        public void Execute(int index)
        {
            var indices = Chunks[index].Comp.ChunkIndices;
            var chunkBuffer = Chunks[index].Comp.ChunkBuffer;
            var chunkGasBuffer = Chunks[index].Comp.GasArrayPool;
            chunkBuffer.Clear();
            SubGridSystem.ResolveMapRelativeToChunk(SubGrid, ref chunkBuffer, indices);
            AtmosphericsSystem.ProcessDiffusionChunk(chunkBuffer, indices, DeltaTime, chunkGasBuffer);
        }
    }

    public void ProcessDiffusionChunk(
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

            chunkData.AtmosphereMap[i] = ProcessDiffusionTile(tile, i, chunkBuffer, indices, deltaTime, pool);
        }
    }

    private TileAtmos ProcessDiffusionTile(
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

    private record struct ProcessSimpleAtmosMovement(SharedSubGridSystem SubGridSystem, AtmosphericsSystem AtmosphericsSystem) : IParallelRobustJob
    {
        public int BatchSize => 1;

        public readonly SharedSubGridSystem SubGridSystem = SubGridSystem;

        public readonly AtmosphericsSystem AtmosphericsSystem = AtmosphericsSystem;

        public Entity<SubGridComponent> SubGrid;

        public ValueList<Entity<SubGridChunkComponent>> Chunks = new();

        public float DeltaTime;

        public void Execute(int index)
        {
            var indices = Chunks[index].Comp.ChunkIndices;
            var chunkBuffer = Chunks[index].Comp.ChunkBuffer;
            var chunkGasBuffer = Chunks[index].Comp.GasArrayPool;
            chunkBuffer.Clear();
            SubGridSystem.ResolveMapRelativeToChunk(SubGrid, ref chunkBuffer, indices);
            AtmosphericsSystem.ProcessSimpleAtmosMovementChunk(chunkBuffer, indices, DeltaTime, chunkGasBuffer);
        }
    }

    public void ProcessSimpleAtmosMovementChunk(
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

            chunkData.AtmosphereMap[i] = ProcessSimpleAtmosMovementChunkTile(tile, i, chunkBuffer, indices, deltaTime, pool);
        }
    }

    private TileAtmos ProcessSimpleAtmosMovementChunkTile(
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

            GasManager.ShareTiles(
                ref tile,
                found.Value,
                _subGrid.SubGridWorldSize,
                alteredTime,
                AtmosTransferCoefficient,
                pool);
        }

        return tile;
    }

    public void ProcessAtmosHeatConductionChunk(
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

            chunkData.AtmosphereMap[i] = ProcessAtmosHeatConductionTile(tile, i, chunkBuffer, indices, deltaTime, pool);
        }
    }

    private TileAtmos ProcessAtmosHeatConductionTile(
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

            GasManager.ShareTiles(
                ref tile,
                found.Value,
                _subGrid.SubGridWorldSize,
                alteredTime,
                AtmosTransferCoefficient,
                pool);
        }

        return tile;
    }
}
