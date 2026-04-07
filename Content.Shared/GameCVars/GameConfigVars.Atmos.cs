using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<bool> AtmosDiffusionEnabled =
        CVarDef.Create("atmos.diffusion.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<bool> AtmosMovementEnabled =
        CVarDef.Create("atmos.movement.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<float> AtmosTransferCoefficient =
        CVarDef.Create("atmos.movement.transfer_coefficient", 1f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> AtmosSpeedup =
        CVarDef.Create("atmos.speedup", 1f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<int> AtmosSteps =
        CVarDef.Create("atmos.steps", 3, CVar.SERVER | CVar.REPLICATED);
}
