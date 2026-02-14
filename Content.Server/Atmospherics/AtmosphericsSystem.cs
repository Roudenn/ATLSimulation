using Content.Server.SubGrid;
using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Systems;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem : SharedAtmosphericsSystem
{
    [Dependency] private readonly SubGridSystem _subGrid = default!;

    public bool AtmosEnabled;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }
}
