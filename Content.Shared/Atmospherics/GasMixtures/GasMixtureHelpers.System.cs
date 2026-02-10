using Content.Shared.Atmospherics.Systems;
using Content.Shared.Constants;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.GasMixtures;

/// This part contains the mirror of <see cref="SharedAtmosphericsSystem"/>'s API.
/// Parameters here require information from the prototypes of each contained gas.
public static partial class GasMixtureHelpers
{
    /// <summary>
    /// Updates the heat capacity of a mixture.
    /// </summary>
    /// <param name="m">The target gas mixture.</param>
    /// <param name="gasSpecificHeats">An array of specific heat capacities of all gases, usually available from <see cref="SharedAtmosphericsSystem"/></param>
    /// <returns>Calculated heat capacity.</returns>
    [PublicAPI]
    public static float CalculateHeatCapacity(this ref GasMixture m, ref float[] gasSpecificHeats)
    {
        Span<float> tmp = stackalloc float[m.Moles.Length];
        NumericsHelpers.Multiply(m.Moles, gasSpecificHeats, tmp);
        
        return MathF.Max(NumericsHelpers.HorizontalAdd(tmp), SystemConstants.MinimumHeatCapacity);
    }
    
    [PublicAPI]
    public static float GetSpecificHeatCapacity(ref GasMixture m, ref float[] gasSpecificHeats)
    {
        Span<float> tmp = stackalloc float[m.Moles.Length];
        NumericsHelpers.Multiply(m.MolesRatio, gasSpecificHeats, tmp);
        return MathF.Max(NumericsHelpers.HorizontalAdd(tmp), SystemConstants.MinimumHeatCapacity);
    }
    
    [PublicAPI]
    public static float GetMass(ref GasMixture m, ref float[] gasMolarMasses)
    {
        Span<float> tmp = stackalloc float[m.Moles.Length];
        NumericsHelpers.Multiply(m.Moles, gasMolarMasses, tmp);
        NumericsHelpers.Multiply(m.Moles, 0.001f, tmp); // Convert grams to kilograms, doing it here for maximum accuracy
        return NumericsHelpers.HorizontalAdd(tmp);
    }
    
    [PublicAPI]
    public static float GetDensity(ref GasMixture m, ref float[] gasMolarMasses)
    {
        return GetMass(ref m, ref gasMolarMasses) / m.Volume;
    }
    
    /// <summary>
    /// Gets thermal conductivity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public static float GetThermalConductivity(ref GasMixture m, ref float[] gasMolarMassesSquareRoots, ref float[] gasThermalConductivities)
    {
        Span<float> tmp1 = stackalloc float[m.Moles.Length];
        Span<float> tmp2 = stackalloc float[m.Moles.Length];
        
        NumericsHelpers.Multiply(m.MolesRatio, gasMolarMassesSquareRoots, tmp1);
        NumericsHelpers.Multiply(tmp1, gasThermalConductivities);
        
        NumericsHelpers.Multiply(m.MolesRatio, gasMolarMassesSquareRoots, tmp2);
        
        return NumericsHelpers.HorizontalAdd(tmp1) / NumericsHelpers.HorizontalAdd(tmp2);
    }
    
    /// <summary>
    /// Gets viscosity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public static float GetViscosity(ref GasMixture m, ref float[] gasMolarMassesSquareRoots, ref float[] gasViscosities)
    {
        Span<float> tmp1 = stackalloc float[m.Moles.Length];
        Span<float> tmp2 = stackalloc float[m.Moles.Length];
        
        NumericsHelpers.Multiply(m.MolesRatio, gasMolarMassesSquareRoots, tmp1);
        NumericsHelpers.Multiply(tmp1, gasViscosities);
        
        NumericsHelpers.Multiply(m.MolesRatio, gasMolarMassesSquareRoots, tmp2);
        
        return NumericsHelpers.HorizontalAdd(tmp1) / NumericsHelpers.HorizontalAdd(tmp2);
    }
}
