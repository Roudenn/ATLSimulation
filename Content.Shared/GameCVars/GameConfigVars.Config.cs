using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<string> ConfigPresets =
        CVarDef.Create("config.presets", "default", CVar.SERVERONLY);
}