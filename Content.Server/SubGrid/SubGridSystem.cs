using Content.Server.Atmospherics;
using Content.Server.Statistics;
using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Components;
using Content.Shared.Materials;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.SubGrid;

public sealed partial class SubGridSystem : SharedSubGridSystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [Dependency] private readonly AtmosphericsSystem _atmos = default!;

    private EntityQuery<HeatContainerComponent> _temperatureQuery;
    private EntityQuery<MaterialComponent> _materialQuery;

    public override void Initialize()
    {
        base.Initialize();

        InitializeChunks();

        SubscribeLocalEvent<GetStatisticsEvent>(OnGetStats);
        _temperatureQuery = GetEntityQuery<HeatContainerComponent>();
        _materialQuery = GetEntityQuery<MaterialComponent>();
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
            foreach (var atmosTile in chunkComp.AtmosphereMap)
            {
                if (!atmosTile.Initialized)
                    continue;

                tileCount++;
            }

            foreach (var temperatureTile in chunkComp.TemperatureMap)
            {
                if (!temperatureTile.Initialized)
                    continue;

                tileCount++;
            }
        }

        ev.Stats.ChunkCount = chunkCount;
        ev.Stats.TileCount = tileCount;
    }

    /*public override void Update(float frameTime)
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
    }*/
}
