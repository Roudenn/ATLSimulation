using System.Numerics;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmospherics.GasMixtures;

/// <summary>
/// A general-purpose container for gases.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
[Access(typeof(GasMixtureFactory))]
public partial struct VelocityGasMixture : IGasMixture, IRobustCloneable<VelocityGasMixture>
{
    /// <summary>
    /// Contains an amount of every single gas in moles.
    /// </summary>
    [DataField(customTypeSerializer: typeof(GasArraySerializer))]
    public float[] Moles { get; set; }

    /// <summary>
    /// Current velocity of gas in meters per second.
    /// </summary>
    [DataField]
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// Volume of the mixture's container.
    /// </summary>
    [DataField]
    public float Volume { get; set; } = 1f;

    /// <summary>
    /// Temperature of the mixture in kelvins.
    /// </summary>
    [DataField]
    public float Temperature { get; set; }

    /// <summary>
    /// The current temperature of the container in Celsius.
    /// Ideal if you just need to read the temperature for UI.
    /// Do not perform computations in Celsius/set this value, use Kelvin instead.
    /// </summary>
    [ViewVariables]
    public float TemperatureC => TemperatureHelpers.KelvinToCelsius(Temperature);

    /// <summary>
    /// If true, any attempts to modify the mixture will be cancelled.
    /// Immutable fixtures are the main reason why first law of thermodynamics is technically violated.
    /// </summary>
    [DataField]
    public bool Immutable { get; set; }

    [ViewVariables, Access(typeof(SharedSubGridSystem))]
    public bool Initialized = true;

    /// <summary>
    /// Constructs a new gas mixture without any gas.
    /// </summary>
    public VelocityGasMixture(int size, float volume, float temperature, Vector2 velocity, bool immutable = false)
    {
        Moles = new float[size];
        Volume = volume;
        Temperature = temperature;
        Velocity = velocity;
        Immutable = immutable;
    }

    /// <summary>
    /// Constructs a new gas mixture out of an array of moles and a heat container.
    /// </summary>
    /// <remarks>
    /// Ensure that the array size is correct and heat container has correct heat capacity relatively to the <see cref="Moles"/> array.
    /// </remarks>
    public VelocityGasMixture(float[] moles, float volume, float temperature, Vector2 velocity, bool immutable = false)
    {
        Moles = moles;
        Volume = volume;
        Temperature = temperature;
        Velocity = velocity;
        Immutable = immutable;
    }

    public VelocityGasMixture(GasMixture m, Vector2 velocity)
    {
        Moles = m.Moles;
        Volume = m.Volume;
        Temperature = m.Temperature;
        Velocity = velocity;
        Immutable = m.Immutable;
    }

    public VelocityGasMixture(VelocityGasMixture m)
    {
        Moles = m.Moles;
        Volume = m.Volume;
        Temperature = m.Temperature;
        Velocity = m.Velocity;
        Immutable = m.Immutable;
    }

    public VelocityGasMixture Clone()
    {
        return new VelocityGasMixture(this);
    }
}
