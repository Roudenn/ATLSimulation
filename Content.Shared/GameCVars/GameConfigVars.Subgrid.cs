using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<int> SubGridSize =
        CVarDef.Create("subgrid.size", 2, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);
}
