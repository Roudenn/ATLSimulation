using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<bool> AtmosEnabled =
        CVarDef.Create("atmos.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<float> AtmosSpeedup =
        CVarDef.Create("atmos.speedup", 1f, CVar.SERVER | CVar.REPLICATED);
}
