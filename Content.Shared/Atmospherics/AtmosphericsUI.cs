using Robust.Shared.Serialization;

namespace Content.Shared.Atmospherics;

/// <summary>
/// Contains information on a gas mix entry, turns into a tab in the UI
/// </summary>
[Serializable, NetSerializable]
public struct GasMixEntry
{
    /// <summary>
    /// Name of the tab in the UI
    /// </summary>
    public readonly string Name;

    public readonly float Volume;
    public readonly float Pressure;
    public readonly float Temperature;
    public readonly GasEntry[]? Gases;

    public GasMixEntry(string name, float volume, float pressure, float temperature, GasEntry[]? gases = null)
    {
        Name = name;
        Volume = volume;
        Pressure = pressure;
        Temperature = temperature;
        Gases = gases;
    }
}

/// <summary>
/// Individual gas entry data for populating the UI
/// </summary>
[Serializable, NetSerializable]
public struct GasEntry
{
    public readonly string Name;
    public readonly float Amount;
    public readonly string Color;

    public GasEntry(string name, float amount, string color)
    {
        Name = name;
        Amount = amount;
        Color = color;
    }

    public override string ToString()
    {
        // e.g. "Plasma: 2000 mol"
        return Loc.GetString(
            "gas-entry-info",
            ("gasName", Name),
            ("gasAmount", Amount));
    }
}
