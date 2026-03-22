using Content.Shared.GameCVars;
using Robust.Shared.Configuration;

namespace Content.Shared.Temperature.Systems;

public abstract class SharedTemperatureSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;

    [ViewVariables]
    public bool TemperatureEnabled;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(CfgManager, GameConfigVars.TemperatureEnabled, b => TemperatureEnabled = b, true);
    }
}
