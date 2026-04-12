using Content.Server.Atmospherics;
using Content.Server.Statistics;
using Content.Server.Temperature;
using Content.Shared.Atmospherics.Components;
using Content.Shared.Materials;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.SubGrid;

public sealed partial class SubGridSystem : SharedSubGridSystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AtmosphericsSystem _atmos = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<HeatContainerComponent> _temperatureQuery;
    private EntityQuery<MaterialComponent> _materialQuery;
    private EntityQuery<GasMarkerComponent> _markerQuery;

    private GameTick _lastDirtyTick;

    public override void Initialize()
    {
        base.Initialize();

        InitializeChunks();

        SubscribeLocalEvent<GetStatisticsEvent>(OnGetStats);
        _temperatureQuery = GetEntityQuery<HeatContainerComponent>();
        _materialQuery = GetEntityQuery<MaterialComponent>();
        _markerQuery = GetEntityQuery<GasMarkerComponent>();
    }

    private void OnGetStats(ref GetStatisticsEvent ev)
    {
        var chunkCount = 0;
        var tileCount = 0;

        var chunkQuery = EntityQueryEnumerator<SubGridChunkComponent>();
        while (chunkQuery.MoveNext(out var chunkComp))
        {
            chunkCount++;
            // TODO implement a better way to count active tiles
            foreach (var atmosTile in chunkComp.ChunkData.AtmosphereMap)
            {
                if (!atmosTile.Initialized)
                    continue;

                tileCount++;
            }

            foreach (var temperatureTile in chunkComp.ChunkData.TemperatureMap)
            {
                if (!temperatureTile.Initialized)
                    continue;

                tileCount++;
            }
        }

        ev.Stats.ChunkCount = chunkCount;
        ev.Stats.TileCount = tileCount;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _temperature.UpdateHeat();
        _atmos.UpdateAtmos();

        var curTick = _timing.CurTick;
        if (curTick.Value - _lastDirtyTick.Value < SubGridNetFrequency)
            return;

        _lastDirtyTick = curTick;
        var query = EntityQueryEnumerator<SubGridComponent>();
        while (query.MoveNext(out var comp))
        {
            foreach (var chunkIndices in comp.ChunkEntities.Keys)
            {
                var chunkEnt = comp.ChunkEntities[chunkIndices];
                if (!ChunkQuery.TryComp(chunkEnt, out var chunkComp))
                    continue;

                Dirty(chunkEnt, chunkComp);
            }
        }
    }
}
