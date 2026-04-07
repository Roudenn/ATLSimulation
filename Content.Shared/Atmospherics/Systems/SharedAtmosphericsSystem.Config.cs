using Content.Shared.GameCVars;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem
{
    [ViewVariables]
    public bool AtmosMovementEnabled;

    [ViewVariables]
    public bool AtmosDiffusionEnabled;

    [ViewVariables]
    public float AtmosSpeedup;

    [ViewVariables]
    public int AtmosSteps;

    [ViewVariables]
    public float AtmosTransferCoefficient;

    private void InitializeCVars()
    {
        Subs.CVar(_config, GameConfigVars.AtmosMovementEnabled, b => AtmosMovementEnabled = b, true);
        Subs.CVar(_config, GameConfigVars.AtmosDiffusionEnabled, b => AtmosDiffusionEnabled = b, true);
        Subs.CVar(_config, GameConfigVars.AtmosSpeedup, f => AtmosSpeedup = f, true);
        Subs.CVar(_config, GameConfigVars.AtmosTransferCoefficient, f => AtmosTransferCoefficient = f, true);
        Subs.CVar(_config, GameConfigVars.AtmosSteps, i => AtmosSteps = i, true);
    }
}
