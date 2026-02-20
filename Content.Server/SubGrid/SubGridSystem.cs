using Content.Server.Atmospherics;
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
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly AtmosphericsSystem _atmos = default!;

    private EntityQuery<MapGridComponent> _mapGridQuery;
    private EntityQuery<GridAtmosphereComponent> _atmosGridQuery;
    private EntityQuery<TemperatureContainerComponent> _temperatureQuery;
    private EntityQuery<MaterialComponent> _materialQuery;

    public override void Initialize()
    {
        base.Initialize();

        InitializeChunks();

        _mapGridQuery = GetEntityQuery<MapGridComponent>();
        _atmosGridQuery = GetEntityQuery<GridAtmosphereComponent>();
        _temperatureQuery = GetEntityQuery<TemperatureContainerComponent>();
        _materialQuery = GetEntityQuery<MaterialComponent>();
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
