using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    [PublicAPI]
    public float GetMoles<T>(ref T m, int gasId) where T : IGasMixture
    {
        return m.Moles[gasId];
    }

    [PublicAPI]
    public float GetTotalMoles<T>(ref T m) where T : IGasMixture
        => NumericsHelpers.HorizontalAdd(m.Moles);

    [PublicAPI]
    public void GetMolesRatio<T>(ref T m, ref float[] ratios) where T : IGasMixture
    {
        NumericsHelpers.Divide(m.Moles, GetTotalMoles(ref m), ratios);
    }

    [PublicAPI]
    public float GetPressure<T>(ref T m) where T : IGasMixture
        => GetTotalMoles(ref m) * m.Temperature * PhysicalConstants.R / m.Volume;

    /// <summary>
    /// Gets the heat capacity of a mixture.
    /// </summary>
    /// <param name="m">The target gas mixture.</param>
    /// <returns>Calculated heat capacity.</returns>
    [PublicAPI]
    public float GetHeatCapacity<T>(ref T m) where T : IGasMixture
    {
        NumericsHelpers.Multiply(m.Moles, GasSpecificHeats, GasBuffer);
        var result = MathF.Max(NumericsHelpers.HorizontalAdd(GasBuffer), SystemConstants.MinimumHeatCapacity);
        ClearBuffer(ref GasBuffer);
        return result;
    }

    [PublicAPI]
    public float GetInternalEnergy<T>(ref T m) where T : IGasMixture
    {
        return m.Temperature * GetHeatCapacity(ref m);
    }

    [PublicAPI]
    public float GetSpecificHeatCapacity<T>(ref T m) where T : IGasMixture
    {
        GetMolesRatio(ref m, ref GasBufferRatios);
        NumericsHelpers.Multiply(GasBufferRatios, GasSpecificHeats, GasBuffer);
        var result = MathF.Max(NumericsHelpers.HorizontalAdd(GasBuffer), SystemConstants.MinimumHeatCapacity);
        ClearBuffer(ref GasBuffer, ref GasBufferRatios);
        return result;
    }

    /// <summary>
    /// Gets thermal conductivity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public float GetThermalConductivity<T>(ref T m) where T : IGasMixture
    {
        GetMolesRatio(ref m, ref GasBufferRatios);
        NumericsHelpers.Multiply(GasBufferRatios, GasMolarMassesSquareRoots, GasBuffer);
        NumericsHelpers.Multiply(GasBuffer, GasThermalConductivities, GasBuffer2);
        var result = NumericsHelpers.HorizontalAdd(GasBuffer) / NumericsHelpers.HorizontalAdd(GasBuffer2);
        ClearBuffer(ref GasBuffer, ref GasBuffer2, ref GasBufferRatios);
        return result;
    }

    [PublicAPI]
    public float GetMass<T>(ref T m) where T : IGasMixture
    {
        NumericsHelpers.Multiply(m.Moles, GasMolarMasses, GasBuffer);
        NumericsHelpers.Multiply(m.Moles, PhysicalConstants.GramsToKilograms, GasBuffer);
        var result = NumericsHelpers.HorizontalAdd(GasBuffer);
        ClearBuffer(ref GasBuffer);
        return result;
    }

    [PublicAPI]
    public float GetDensity<T>(ref T m) where T : IGasMixture
    {
        return GetMass(ref m) / m.Volume;
    }

    /// <summary>
    /// Gets viscosity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public float GetViscosity<T>(ref T m) where T : IGasMixture
    {
        GetMolesRatio(ref m, ref GasBufferRatios);
        NumericsHelpers.Multiply(GasBufferRatios, GasMolarMassesSquareRoots, GasBuffer);
        NumericsHelpers.Multiply(GasBuffer, GasViscosities, GasBuffer2);
        var result = NumericsHelpers.HorizontalAdd(GasBuffer) / NumericsHelpers.HorizontalAdd(GasBuffer2);
        ClearBuffer(ref GasBuffer, ref GasBuffer2, ref GasBufferRatios);
        return result;
    }

    /// <summary>
    /// Gets the prandtl number of a mixture.
    /// </summary>
    /// <param name="m">The target gas mixture.</param>
    /// <returns>Calculated prandtl number.</returns>
    /// <remarks>
    /// This is an expensive operation! If you've already calculated conductivity and viscosity, use an overload.
    /// </remarks>
    [PublicAPI]
    public float GetPrantlNumber<T>(ref T m) where T : IGasMixture
    {
        // If that will be a hotspot, then some code here should be unwrapped to make shortcuts.
        var thermalConductivity = GetThermalConductivity(ref m);
        var viscosity = GetViscosity(ref m);
        return GetHeatCapacity(ref m) * viscosity / thermalConductivity;
    }

    [PublicAPI]
    public float GetPrantlNumber<T>(ref T m, float thermalConductivity, float viscosity) where T : IGasMixture
        => GetHeatCapacity(ref m) * viscosity / thermalConductivity;
}
