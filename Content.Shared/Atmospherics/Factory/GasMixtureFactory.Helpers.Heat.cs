using Content.Shared.Atmospherics.GasMixtures;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    /// <summary>
    /// Adds or removes heat energy from the <see cref="IGasMixture"/>.
    /// Positive values add heat, negative values remove heat.
    /// The temperature can never become lower than 0K even if more heat is removed.
    /// </summary>
    /// <param name="m">The <see cref="IGasMixture"/> to add or remove energy.</param>
    /// <param name="dQ">The energy in joules to add or remove.</param>
    [PublicAPI]
    public void AddHeat<T>(ref T m, float dQ) where T : IGasMixture
    {
        m.Temperature = AddHeatQuery(ref m, dQ);
    }

    /// <summary>
    /// Calculates the resulting temperature of the container after adding or removing heat energy.
    /// Positive values add heat, negative values remove heat. This method doesn't change the container's state.
    /// The temperature can never become lower than 0K even if more heat is removed.
    /// </summary>
    /// <param name="m">The <see cref="IGasMixture"/> to query.</param>
    /// <param name="dQ">The energy in joules to add or remove.</param>
    /// <returns>The resulting temperature in kelvin after the heat change.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the heat capacity of the container is zero or negative.</exception>
    [PublicAPI]
    public float AddHeatQuery<T>(ref T m, float dQ) where T : IGasMixture
    {
        // Don't allow the temperature to go below the absolute minimum.
        return Math.Max(0f, m.Temperature + dQ / GetHeatCapacity(ref m));
    }
}
