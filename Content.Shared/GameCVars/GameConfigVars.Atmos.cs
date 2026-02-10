using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<bool> AtmosEnabled =
        CVarDef.Create("atmos.enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);
    
    public static readonly CVarDef<bool> AtmosEightDirections =
        CVarDef.Create("atmos.eight_directions", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE,
            "If true, the atmospheric simulation will use 8 directions to calculate gas flow.");
}
