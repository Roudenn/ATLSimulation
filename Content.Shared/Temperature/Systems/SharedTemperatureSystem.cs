using Content.Shared.GameCVars;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared.Temperature.Systems;

public abstract class SharedTemperatureSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;

    [ViewVariables]
    public bool HeatEnabled;

    [ViewVariables]
    public float HeatSpeedup;

    [ViewVariables]
    public int HeatSteps;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(CfgManager, GameConfigVars.HeatEnabled, b => HeatEnabled = b, true);
        Subs.CVar(CfgManager, GameConfigVars.HeatSpeedup, f => HeatSpeedup = f, true);
        Subs.CVar(CfgManager, GameConfigVars.HeatSteps, i => HeatSteps = i, true);
    }

    public virtual void AddHeatArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, float energy) { }
}
