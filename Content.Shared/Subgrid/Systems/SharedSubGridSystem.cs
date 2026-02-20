using Content.Shared.Subgrid.Components;
using Robust.Shared.Configuration;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;

    protected EntityQuery<SubGridComponent> SubGridQuery;
    protected EntityQuery<SubGridChunkComponent> ChunkQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        InitializeCVars();
        InitializeUI();

        SubGridQuery = GetEntityQuery<SubGridComponent>();
        ChunkQuery = GetEntityQuery<SubGridChunkComponent>();
    }

    public static readonly Vector2i[] Directions = new[]
    {
        Vector2i.Up,
        Vector2i.Right,
        Vector2i.Down,
        Vector2i.Left,
    };

    public static readonly Vector2i[] DirectionsWithDiagonals = new[]
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
