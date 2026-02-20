using Content.Shared.Subgrid.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem : EntitySystem
{
    // TODO port stuff needed from the server system to here
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly GasPrototypeManager _gasManager = default!;
    [Dependency] private readonly SharedSubGridSystem _subGrid = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeCVars();
        InitializeGases();
    }
}
