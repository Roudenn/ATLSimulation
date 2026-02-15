using Content.Server.Atmospherics;
using Content.Shared.Atmospherics;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;
using Robust.Shared.Map.Components;

namespace Content.Server.SubGrid;

public sealed partial class SubGridSystem : SharedSubGridSystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly AtmosphericsSystem _atmos = default!;

    private EntityQuery<SubGridComponent> _subGridQuery;
    private EntityQuery<MapGridComponent> _mapGridQuery;

    private Dictionary<Vector2i, TileAtmosphere[]> AtmosphereCache = new(256);
    private Dictionary<Vector2i, TileTemperature[]> TemperatureCache = new(256);

    public override void Initialize()
    {
        base.Initialize();

        InitializeChunks();

        _subGridQuery = GetEntityQuery<SubGridComponent>();
        _mapGridQuery = GetEntityQuery<MapGridComponent>();
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
