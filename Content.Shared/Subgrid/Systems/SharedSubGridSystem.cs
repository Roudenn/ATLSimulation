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
}
