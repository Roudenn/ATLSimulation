using System.Linq;
using Content.Server.SubGrid;
using Content.Shared.Subgrid;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Systems;
using Robust.Shared.Threading;
using Robust.Shared.Timing;

namespace Content.Server.Temperature;

public sealed class TemperatureSystem : SharedTemperatureSystem
{
    [Dependency] private readonly SubGridSystem _subGrid = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;

    private EntityQuery<SubGridChunkComponent> _subgridChunkQuery;

    private TimeSpan _lastUpdate = TimeSpan.Zero;

    private ProcessTemperature _job;

    public override void Initialize()
    {
        base.Initialize();
        _subgridChunkQuery = GetEntityQuery<SubGridChunkComponent>();
        _job = new(_subGrid, this);
    }

    public void UpdateHeat()
    {
        if (!HeatEnabled)
            return;

        var query = EntityQueryEnumerator<SubGridComponent>();
        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
        deltaTime = MathF.Min(deltaTime, 0.01f);
        _lastUpdate = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            var indicies = comp.ChunkEntities.Keys.ToList();

            _job.SubGrid = (uid, comp);
            _job.ChunkIndices = indicies;
            _job.DeltaTime = deltaTime / HeatSteps;
            for (int i = 0; i < HeatSteps; i++)
            {
                _parallel.ProcessNow(_job, indicies.Count);

                foreach (var chunkIndices in indicies)
                {
                    var chunkEnt = comp.ChunkEntities[chunkIndices];
                    if (!_subgridChunkQuery.TryComp(chunkEnt, out var chunkComp))
                        continue;

                    for (var index = 0; index < chunkComp.ChunkData.TemperatureMap.Length; index++)
                    {
                        chunkComp.ChunkData.TemperatureMap[index].ArchivedContainer = chunkComp.ChunkData.TemperatureMap[index].Container;
                    }
                }
            }
        }
    }

    private record struct ProcessTemperature(SharedSubGridSystem SubGridSystem, TemperatureSystem TemperatureSystem) : IParallelRobustJob
    {
        public int BatchSize => 1;

        public readonly SharedSubGridSystem SubGridSystem = SubGridSystem;

        public readonly TemperatureSystem TemperatureSystem = TemperatureSystem;

        public Entity<SubGridComponent> SubGrid;

        public List<Vector2i> ChunkIndices = new();

        public float DeltaTime;

        public void Execute(int index)
        {
            var indices = ChunkIndices[index];
            var chunkBuffer = SubGrid.Comp.ChunkMapCaches[indices];
            chunkBuffer.Clear();
            SubGridSystem.ResolveMapRelativeToChunk(SubGrid, ref chunkBuffer, indices);
            TemperatureSystem.ProcessChunk(chunkBuffer, indices, DeltaTime);
        }
    }

    public void ProcessChunk(Dictionary<Vector2i, SubGridChunk> chunkBuffer, Vector2i indices, float deltaTime)
    {
        var chunkData = chunkBuffer[indices];
        for (var i = 0; i < chunkData.TemperatureMap.Length; i++)
        {
            var tile = chunkData.TemperatureMap[i];
            if (!tile.Initialized)
                continue;

            chunkData.TemperatureMap[i] = ProcessTile(chunkBuffer, tile, indices, i, deltaTime);
        }
    }

    private TileHeat ProcessTile(Dictionary<Vector2i, SubGridChunk> chunkBuffer, TileHeat tile, Vector2i chunkIndices, int index, float deltaTime)
    {
        foreach (var dir in SharedSubGridSystem.DirectionsWithDiagonals)
        {
            if (!_subGrid.TryGetHeatTileRelative(chunkBuffer, chunkIndices, index, dir, out var found))
                continue;

            // The coefficients for interaction get halved when going diagonally,
            // since Characteristic Length is multiplied by √2 and surface area is multiplied by √2/2.
            var alteredTime = deltaTime * HeatSpeedup;
            if (!SharedSubGridSystem.Directions.Contains(dir))
            {
                alteredTime /= 2f;

                // Diagonal movement is only possible if there are also neighbouring tiles.
                if (!_subGrid.TryGetHeatTileRelative(chunkBuffer, chunkIndices, index, new Vector2i(dir.X, 0), out var foundFirst)
                    || !_subGrid.TryGetHeatTileRelative(chunkBuffer, chunkIndices, index, new Vector2i(0, dir.Y), out var foundSecond)
                    || !foundFirst.Value.Initialized
                    || !foundSecond.Value.Initialized)
                    continue;
            }

            TileHeatHelpers.ConductHeatTiles(ref tile, found.Value, alteredTime);
        }

        return tile;
    }
}
