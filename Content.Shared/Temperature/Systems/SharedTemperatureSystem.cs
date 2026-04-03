using Content.Shared.GameCVars;
using Robust.Shared.Configuration;

namespace Content.Shared.Temperature.Systems;

public abstract class SharedTemperatureSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;

    [ViewVariables]
    public bool TemperatureEnabled;

    [ViewVariables]
    public float TemperatureSpeedup;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(CfgManager, GameConfigVars.TemperatureEnabled, b => TemperatureEnabled = b, true);
        Subs.CVar(CfgManager, GameConfigVars.TemperatureSpeedup, f => TemperatureSpeedup = f, true);
    }
}
