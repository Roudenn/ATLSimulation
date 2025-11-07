using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

// DEVNOTE: This is the same as SS14's CCVars. Except it's not named CCVars as that name is 
// hot garbage.
[CVarDefs]
public sealed partial class GameConfigVars : CVars
{
    public static readonly CVarDef<bool> ParallaxEnabled =
        CVarDef.Create("parallax.enabled", true, CVar.CLIENTONLY);

    public static readonly CVarDef<bool> ParallaxDebug =
        CVarDef.Create("parallax.debug", false, CVar.CLIENTONLY);

    public static readonly CVarDef<bool> ParallaxLowQuality =
        CVarDef.Create("parallax.low_quality", false, CVar.ARCHIVE | CVar.CLIENTONLY);
    
    /// <summary>
    ///     Displays framerate counter
    /// </summary>
    public static readonly CVarDef<bool> HudFpsCounterVisible =
        CVarDef.Create("hud.fps_counter_visible", false, CVar.CLIENTONLY | CVar.ARCHIVE);
    
    /// <summary>
    ///     Toggles whether the walking key is a toggle or a held key.
    /// </summary>
    public static readonly CVarDef<bool> ToggleWalk =
        CVarDef.Create("control.toggle_walk", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}