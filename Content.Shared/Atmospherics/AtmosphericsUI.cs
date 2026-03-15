using System.Numerics;

namespace Content.Shared.Atmospherics;

/// <summary>
/// Contains information on a gas mix entry, turns into a tab in the UI
/// </summary>
public struct GasMixEntry(
    float volume,
    float pressure,
    float temperature,
    float heatCapacity,
    float thermalConductivity,
    float mass,
    GasEntry[]? gases = null,
    Vector2? speed = null)
{
    public readonly float Volume = volume;
    public readonly float Pressure = pressure;
    public readonly float Temperature = temperature;
    public readonly float HeatCapacity = heatCapacity;
    public readonly float ThermalConductivity = thermalConductivity;
    public readonly float Mass = mass;
    public readonly GasEntry[]? Gases = gases;
}

/// <summary>
/// Individual gas entry data for populating the UI
/// </summary>
public struct GasEntry
{
    public readonly LocId Name;
    public readonly float Amount;
    public readonly Color Color;

    public GasEntry(LocId name, float amount, Color color)
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
