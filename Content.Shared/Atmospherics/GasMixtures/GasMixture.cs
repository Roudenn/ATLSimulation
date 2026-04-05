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
public partial struct GasMixture : IGasMixture, IRobustCloneable<GasMixture>
{
    /// <summary>
    /// Contains an amount of every single gas in moles.
    /// </summary>
    [DataField(customTypeSerializer: typeof(GasArraySerializer))]
    public float[] Moles { get; set; }

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

    // TODO this is a workaround until actually good serialization is made
    [ViewVariables, Access(typeof(SharedSubGridSystem))]
    public bool Initialized = true;

    /// <summary>
    /// Constructs a new gas mixture without any gas.
    /// </summary>
    public GasMixture(int size, float volume, float temperature, bool immutable = false)
    {
        Moles = new float[size];
        Volume = volume;
        Temperature = temperature;
        Immutable = immutable;
    }

    /// <summary>
    /// Constructs a new gas mixture out of an array of moles and a heat container.
    /// </summary>
    /// <remarks>
    /// Ensure that the array size is correct and heat container has correct heat capacity relatively to the <see cref="Moles"/> array.
    /// </remarks>
    public GasMixture(float[] moles, float volume, float temperature, bool immutable = false)
    {
        Moles = moles;
        Volume = volume;
        Temperature = temperature;
        Immutable = immutable;
    }

    public GasMixture(GasMixture m)
    {
        Moles = m.Moles;
        Volume = m.Volume;
        Temperature = m.Temperature;
        Immutable = m.Immutable;
    }

    public GasMixture Clone()
    {
        return new GasMixture(this);
    }
}
