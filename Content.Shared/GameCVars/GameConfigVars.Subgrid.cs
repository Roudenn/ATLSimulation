using Robust.Shared.Configuration;

namespace Content.Shared.GameCVars;

public sealed partial class GameConfigVars
{
    public static readonly CVarDef<int> SubGridSize =
        CVarDef.Create("subgrid.size", 2, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<int> SubGridNetFrequency =
        CVarDef.Create("subgrid.net_frequency", 2, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// The virtual height of the mythical entity called "the floor" that is 1000% fake.
    /// Used for calculating the volume of a subgrid tile.
    /// </summary>
    public static readonly CVarDef<float> SubGridHeight =
        CVarDef.Create("subgrid.height", 2.5f, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);
}
