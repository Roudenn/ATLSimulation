using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<bool> HeatEnabled =
        CVarDef.Create("heat.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<float> HeatSpeedup =
        CVarDef.Create("heat.speedup", 1f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<int> HeatSteps =
        CVarDef.Create("heat.steps", 3, CVar.SERVER | CVar.REPLICATED);

    /*public static readonly CVarDef<bool> TemperatureDirections =
        CVarDef.Create("temperature.eight_directions", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE,
            "If true, the atmospheric simulation will use 8 directions to calculate heat flow.");*/
}
