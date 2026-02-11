using Content.Shared.Atmospherics.Systems;
using Content.Shared.Constants;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.GasMixtures;

/// This part contains the mirror of <see cref="SharedAtmosphericsSystem"/>'s API.
/// Parameters here require information from the prototypes of each contained gas.
public static partial class GasMixtureHelpers
{
    /// <summary>
    /// Gets the heat capacity of a mixture.
    /// </summary>
    /// <param name="m">The target gas mixture.</param>
    /// <param name="buffer">Buffer with the length equal to the number of gases.</param>
    /// <param name="gasSpecificHeats">An array of specific heat capacities of all gases, usually available from <see cref="SharedAtmosphericsSystem"/></param>
    /// <returns>Calculated heat capacity.</returns>
    [PublicAPI]
    public static float GetHeatCapacityQuery(this ref GasMixture m, ref float[] buffer, ref float[] gasSpecificHeats)
    {
        NumericsHelpers.Multiply(m.Moles, gasSpecificHeats, buffer);
        return MathF.Max(NumericsHelpers.HorizontalAdd(buffer), SystemConstants.MinimumHeatCapacity);
    }
    
    [PublicAPI]
    public static float GetSpecificHeatCapacityQuery(this ref GasMixture m, ref float[] buffer, ref float[] gasSpecificHeats)
    {
        NumericsHelpers.Multiply(m.MolesRatio, gasSpecificHeats, buffer);
        return MathF.Max(NumericsHelpers.HorizontalAdd(buffer), SystemConstants.MinimumHeatCapacity);
    }
    
    [PublicAPI]
    public static float GetMassQuery(this ref GasMixture m, ref float[] buffer, ref float[] gasMolarMasses)
    {
        NumericsHelpers.Multiply(m.Moles, gasMolarMasses, buffer);
        NumericsHelpers.Multiply(m.Moles, PhysicalConstants.GramsToKilograms, buffer);
        return NumericsHelpers.HorizontalAdd(buffer);
    }
    
    [PublicAPI]
    public static float GetDensityQuery(this ref GasMixture m, ref float[] buffer, ref float[] gasMolarMasses)
    {
        return GetMassQuery(ref m, ref buffer, ref gasMolarMasses) / m.Volume;
    }
    
    /// <summary>
    /// Gets thermal conductivity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public static float GetThermalConductivityQuery(
        this ref GasMixture m,
        ref float[] buffer1,
        ref float[] buffer2,
        ref float[] gasMolarMassesSquareRoots,
        ref float[] gasThermalConductivities)
    {
        NumericsHelpers.Multiply(m.MolesRatio, gasMolarMassesSquareRoots, buffer1);
        NumericsHelpers.Multiply(buffer1, gasThermalConductivities, buffer2);
        
        return NumericsHelpers.HorizontalAdd(buffer1) / NumericsHelpers.HorizontalAdd(buffer2);
    }

    /// <summary>
    /// Gets viscosity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public static float GetViscosityQuery(
        this ref GasMixture m,
        ref float[] buffer1,
        ref float[] buffer2,
        ref float[] gasMolarMassesSquareRoots,
        ref float[] gasViscosities)
    {
        NumericsHelpers.Multiply(m.MolesRatio, gasMolarMassesSquareRoots, buffer1);
        NumericsHelpers.Multiply(buffer1, gasViscosities, buffer2);
        
        return NumericsHelpers.HorizontalAdd(buffer1) / NumericsHelpers.HorizontalAdd(buffer2);
    }
}
