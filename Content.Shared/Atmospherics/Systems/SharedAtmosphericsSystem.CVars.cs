using Content.Shared.GameCVars;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem
{
    [ViewVariables]
    public bool AtmosEnabled;

    [ViewVariables]
    public float AtmosSpeedup;

    private void InitializeCVars()
    {
        Subs.CVar(_config, GameConfigVars.AtmosEnabled, b => AtmosEnabled = b, true);
        Subs.CVar(_config, GameConfigVars.AtmosSpeedup, b => AtmosSpeedup = b, true);
    }
}
