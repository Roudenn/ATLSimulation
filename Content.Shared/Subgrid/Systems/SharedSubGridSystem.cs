using Content.Shared.Constants;
using Content.Shared.GameCVars;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Configuration;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;

    private EntityQuery<SubGridChunkComponent> _chunkQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        InitializeCVars();

        _chunkQuery = GetEntityQuery<SubGridChunkComponent>();
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
