using Content.Shared.Constants;
using Robust.Shared.Serialization;

namespace Content.Shared.Temperature.HeatContainers;

/// <summary>
/// A general-purpose container for heat energy.
/// Any object that contains, stores, or transfers heat should use a <see cref="HeatContainer"/>
/// instead of implementing its own system.
/// This allows for consistent heat transfer mechanics across different objects and systems.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
[Access(typeof(HeatContainerHelpers))]
public partial struct HeatContainer : IRobustCloneable<HeatContainer>
{
    /// <summary>
    /// The heat capacity of this container in Joules per Kelvin.
    /// This determines how much energy is required to change the temperature of the container.
    /// Higher values mean the container can absorb or release more heat energy
    /// without a significant change in temperature.
    /// </summary>
    [DataField]
    public float HeatCapacity = SystemConstants.MinimumHeatCapacity;

    /// <summary>
    /// The current temperature of the container in Kelvin.
    /// </summary>
    [DataField]
    public float Temperature;

    /// <summary>
    /// The thermal conductance in watt per kelvin.
    /// This describes how well heat flows between the bodies.
    /// Higher values mean container gives away heat faster.
    /// </summary>
    [DataField]
    public float ThermalConductance;

    /// <summary>
    /// If true, the temperature of the heat container cannot be changed.
    /// </summary>
    // TODO make it functional!!!
    [DataField]
    public bool Immutable;

    /// <summary>
    /// The current temperature of the container in Celsius.
    /// Ideal if you just need to read the temperature for UI.
    /// Do not perform computations in Celsius/set this value, use Kelvin instead.
    /// </summary>
    [ViewVariables]
    public float TemperatureC => TemperatureHelpers.KelvinToCelsius(Temperature);

    /// <summary>
    /// The current thermal energy of the container in Joules.
    /// </summary>
    [ViewVariables]
    public float InternalEnergy => Temperature * HeatCapacity;

    public HeatContainer(float heatCapacity, float temperature, float thermalConductance, bool immutable = false)
    {
        HeatCapacity = heatCapacity;
        Temperature = temperature;
        ThermalConductance = thermalConductance;
        Immutable = immutable;
    }

    /// <summary>
    /// Copy constructor for implementing ICloneable.
    /// </summary>
    /// <param name="c">The HeatContainer to copy.</param>
    private HeatContainer(HeatContainer c)
    {
        HeatCapacity = c.HeatCapacity;
        Temperature = c.Temperature;
        ThermalConductance = c.ThermalConductance;
        Immutable = c.Immutable;
    }

    public HeatContainer Clone()
    {
        return new HeatContainer(this);
    }
}
