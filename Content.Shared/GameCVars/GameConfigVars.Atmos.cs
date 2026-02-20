using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<bool> AtmosEnabled =
        CVarDef.Create("atmos.enabled", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);
}
