using Content.Shared.Constants;
using Robust.Shared.Serialization;

namespace Content.Shared.Temperature.HeatContainers;

/// <summary>
/// A general-purpose container for heat energy.
/// This variation of <see cref="HeatContainer"/> also stores conductance inside itself,
/// which is useful if the container is frequently used for heat conduction.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
[Access(typeof(HeatContainerHelpers), typeof(ConductiveHeatContainerHelpers))]
public partial struct ConductiveHeatContainer : IRobustCloneable<ConductiveHeatContainer>, IHeatContainer
{
    /// <summary>
    /// The heat capacity of this container in Joules per Kelvin.
    /// This determines how much energy is required to change the temperature of the container.
    /// Higher values mean the container can absorb or release more heat energy
    /// without a significant change in temperature.
    /// </summary>
    [DataField]
    public float HeatCapacity { get; set; } = SystemConstants.Epsilon;

    /// <summary>
    /// The current temperature of the container in Kelvin.
    /// </summary>
    [DataField]
    public float Temperature { get; set; } = PhysicalConstants.ROOM_TEMPERATURE;

    /// <summary>
    /// The thermal conductance in watt per kelvin.
    /// This describes how well heat flows between the bodies.
    /// Higher values mean container gives away heat faster.
    /// </summary>
    [DataField]
    public float ThermalConductance { get; set; }

    /// <summary>
    /// If true, the temperature of the heat container cannot be changed.
    /// </summary>
    [DataField]
    public bool Immutable { get; set; }

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

    public ConductiveHeatContainer(float heatCapacity, float temperature, float thermalConductance, bool immutable = false)
    {
        HeatCapacity = heatCapacity;
        Temperature = temperature;
        ThermalConductance = thermalConductance;
        Immutable = immutable;
    }

    public ConductiveHeatContainer(HeatContainer c, float thermalConductance)
    {
        HeatCapacity = c.HeatCapacity;
        Temperature = c.Temperature;
        ThermalConductance = thermalConductance;
        Immutable = c.Immutable;
    }

    private ConductiveHeatContainer(ConductiveHeatContainer c)
    {
        HeatCapacity = c.HeatCapacity;
        Temperature = c.Temperature;
        ThermalConductance = c.ThermalConductance;
        Immutable = c.Immutable;
    }

    public ConductiveHeatContainer Clone()
    {
        return new ConductiveHeatContainer(this);
    }
}
