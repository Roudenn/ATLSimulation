using Content.Shared.GameCVars;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem
{
    [ViewVariables]
    public bool AtmosEnabled;

    [ViewVariables]
    public float AtmosSpeedup;

    [ViewVariables]
    public int AtmosSteps;

    private void InitializeCVars()
    {
        Subs.CVar(_config, GameConfigVars.AtmosEnabled, b => AtmosEnabled = b, true);
        Subs.CVar(_config, GameConfigVars.AtmosSpeedup, f => AtmosSpeedup = f, true);
        Subs.CVar(_config, GameConfigVars.AtmosSteps, i => AtmosSteps = i, true);
    }
}
