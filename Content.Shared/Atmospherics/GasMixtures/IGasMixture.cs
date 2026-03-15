using Content.Shared.Temperature;

namespace Content.Shared.Atmospherics.GasMixtures;

// TODO consider making a new layer of abstractions where IHeatContainer gets its parameters
// from methods instead of itself. Im not adding this right now because the abstractions become too difficult at this point.
/// <summary>
/// Interface that defines a general-purpose container for a combination of gases.
/// Any object that contains, stores, or transfers gas should use a <see cref="GasMixture"/>
/// or inherit <see cref="IGasMixture"/> instead of implementing its own system.
/// This allows for consistent gas transfer mechanics across different objects and systems.
/// </summary>
public interface IGasMixture
{
    /// <summary>
    /// Contains an amount of every single gas in moles.
    /// </summary>
    float[] Moles { get; set; }

    /// <summary>
    /// Volume of the mixture's container.
    /// </summary>
    float Volume { get; set; }

    /// <summary>
    /// Temperature of the mixture in kelvins.
    /// </summary>
    float Temperature { get; set; }

    /// <summary>
    /// The current temperature of the container in Celsius.
    /// Ideal if you just need to read the temperature for UI.
    /// Do not perform computations in Celsius/set this value, use Kelvin instead.
    /// </summary>
    float TemperatureC => TemperatureHelpers.KelvinToCelsius(Temperature);

    /// <summary>
    /// If true, any attempts to modify the mixture will be cancelled.
    /// Immutable fixtures are the main reason why first law of thermodynamics is technically violated.
    /// </summary>
    bool Immutable { get; set; }
}
