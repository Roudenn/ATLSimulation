using Content.Shared.Atmospherics.Systems;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;
    [Dependency] protected readonly SharedMapSystem MapSystem = default!;
    [Dependency] private readonly SharedAtmosphericsSystem _atmospherics = default!;

    protected EntityQuery<MapGridComponent> MapGridQuery;
    protected EntityQuery<SubGridComponent> SubGridQuery;
    protected EntityQuery<SubGridChunkComponent> ChunkQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        InitializeCVars();
        InitializeUI();

        MapGridQuery = GetEntityQuery<MapGridComponent>();
        SubGridQuery = GetEntityQuery<SubGridComponent>();
        ChunkQuery = GetEntityQuery<SubGridChunkComponent>();
    }

    public static readonly List<Vector2i> Directions = new()
    {
        Vector2i.Up,
        Vector2i.Right,
        Vector2i.Down,
        Vector2i.Left,
    };

    public static readonly List<Vector2i> DirectionsWithDiagonals = new()
    {
        Vector2i.Up,
        Vector2i.UpRight,
        Vector2i.Right,
        Vector2i.DownRight,
        Vector2i.Down,
        Vector2i.DownLeft,
        Vector2i.Left,
        Vector2i.UpLeft,
    };
}
