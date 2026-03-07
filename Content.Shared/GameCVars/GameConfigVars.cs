using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

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

    public static readonly CVarDef<bool> OutlineEnabled =
        CVarDef.Create("outline.enabled", true, CVar.CLIENTONLY);

    public static readonly CVarDef<int> EntityMenuGroupingType =
        CVarDef.Create("entity_menu", 0, CVar.CLIENTONLY);

    /// <summary>
    ///     Size of the lookup area for adding entities to the context menu
    /// </summary>
    public static readonly CVarDef<float> GameEntityMenuLookup =
        CVarDef.Create("game.entity_menu_lookup", 0.25f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> GameDefaultMap =
        CVarDef.Create("game.default_map", "LargeField", CVar.SERVERONLY);
}
