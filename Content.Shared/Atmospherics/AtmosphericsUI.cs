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
    float viscosity,
    float mass,
    float prandtlNumber,
    GasEntry[]? gases = null,
    Vector2? velocity = null)
{
    public readonly float Volume = volume;
    public readonly float Pressure = pressure;
    public readonly float Temperature = temperature;
    public readonly float HeatCapacity = heatCapacity;
    public readonly float ThermalConductivity = thermalConductivity;
    public readonly float Viscosity = viscosity;
    public readonly float Mass = mass;
    public readonly float PrandtlNumber = prandtlNumber;
    public readonly Vector2? Velocity = velocity;
    public readonly GasEntry[]? Gases = gases;

    public override string ToString()
    {
        var main = $"Volume: {Volume:0.0} \n"
                   + $"Pressure: {Pressure:0.0} \n"
                   + $"Temperature: {Temperature:0.00} \n"
                   + $"Heat Capacity: {HeatCapacity:0.0} \n"
                   + $"Conductivity: {ThermalConductivity:0.000} \n"
                   + $"Viscosity: {Viscosity} \n"
                   + $"Mass: {Mass:0.00} \n"
                   + $"Prandtl Number: {PrandtlNumber:0.000} \n"
                   + (Velocity != Vector2.Zero ? $"Velocity: {Velocity} \n" : "");

        for (int i = 0; i < Gases?.Length; i++)
        {
            var gas = Gases[i];
            main += gas + "\n";
        }

        return main;
    }
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
            ("gasName", Loc.GetString(Name)),
            ("gasAmount", $"{Amount:0.00}"));
    }
}
