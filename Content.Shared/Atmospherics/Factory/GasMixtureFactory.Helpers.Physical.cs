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
    public void GetMolesRatio<T>(ref T m, ref float[] buffer) where T : IGasMixture
        => NumericsHelpers.Divide(m.Moles, GetTotalMoles(ref m), buffer);

    [PublicAPI]
    public float GetPressure<T>(ref T m) where T : IGasMixture
        => GetTotalMoles(ref m) / 1000f * m.Temperature * PhysicalConstants.R / m.Volume;

    [PublicAPI]
    public void GetPartialPressures<T>(ref T m, ref float[] buffer) where T : IGasMixture
        => GetPartialPressures(m.Moles, m.Temperature, m.Volume, ref buffer);

    [PublicAPI]
    public void GetPartialPressures(float[] moles, float temperature, float volume, ref float[] buffer)
        => NumericsHelpers.Multiply(moles, 0.001f * temperature * PhysicalConstants.R / volume, buffer);

    /// <summary>
    /// Gets the heat capacity of a mixture.
    /// </summary>
    /// <param name="m">The target gas mixture.</param>
    /// <returns>Calculated heat capacity.</returns>
    [PublicAPI]
    public float GetHeatCapacity<T>(ref T m) where T : IGasMixture
    {
        NumericsHelpers.Multiply(m.Moles, GasSpecificHeats, GasBuffer1);
        var result = MathF.Max(NumericsHelpers.HorizontalAdd(GasBuffer1), SystemConstants.Epsilon);
        ClearBuffer(ref GasBuffer1);
        return result;
    }

    /// <summary>
    /// Gets the heat capacity of a mixture.
    /// </summary>
    /// <param name="mixture">The array that represents a gas mixture.</param>
    /// <returns>Calculated heat capacity.</returns>
    [PublicAPI]
    public float GetHeatCapacity(ref float[] mixture)
    {
        NumericsHelpers.Multiply(mixture, GasSpecificHeats, GasBuffer1);
        var result = MathF.Max(NumericsHelpers.HorizontalAdd(GasBuffer1), SystemConstants.Epsilon);
        ClearBuffer(ref GasBuffer1);
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
        GetMolesRatio(ref m, ref GasBufferResults1);
        NumericsHelpers.Multiply(GasBufferResults1, GasSpecificHeats, GasBuffer1);
        var result = MathF.Max(NumericsHelpers.HorizontalAdd(GasBuffer1), SystemConstants.Epsilon);
        ClearBuffer(ref GasBuffer1, ref GasBufferResults1);
        return result;
    }

    /// <summary>
    /// Gets thermal conductivity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public float GetThermalConductivity<T>(ref T m) where T : IGasMixture
    {
        NumericsHelpers.Multiply(m.Moles, GasMolarMassesSquareRoots, GasBuffer1);
        NumericsHelpers.Multiply(GasBuffer1, GasThermalConductivities, GasBuffer2);
        var bottomPart = NumericsHelpers.HorizontalAdd(GasBuffer1);
        var topPart = NumericsHelpers.HorizontalAdd(GasBuffer2);
        ClearBuffer(ref GasBuffer1, ref GasBuffer2, ref GasBufferResults1);
        return topPart / MathF.Max(bottomPart, SystemConstants.Epsilon);
    }

    [PublicAPI]
    public float GetMass<T>(ref T m) where T : IGasMixture
    {
        NumericsHelpers.Multiply(m.Moles, GasMolarMasses, GasBuffer1);
        var result = NumericsHelpers.HorizontalAdd(GasBuffer1) / 1000f;
        ClearBuffer(ref GasBuffer1);
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
        NumericsHelpers.Multiply(m.Moles, GasMolarMassesSquareRoots, GasBuffer1);
        NumericsHelpers.Multiply(GasBuffer1, GasViscosities, GasBuffer2);
        var bottomPart = NumericsHelpers.HorizontalAdd(GasBuffer1);
        var topPart = NumericsHelpers.HorizontalAdd(GasBuffer2);
        ClearBuffer(ref GasBuffer1, ref GasBuffer2, ref GasBufferResults1);
        return topPart / MathF.Max(bottomPart, SystemConstants.Epsilon) * 10e-7f;
    }

    /// <summary>
    /// Gets the Prandtl number of a mixture.
    /// </summary>
    /// <param name="m">The target gas mixture.</param>
    /// <returns>Calculated prandtl number.</returns>
    /// <remarks>
    /// This is an expensive operation! If you've already calculated conductivity and viscosity, use an overload.
    /// </remarks>
    [PublicAPI]
    public float GetPrandtlNumber<T>(ref T m) where T : IGasMixture
    {
        // If that will be a hotspot, then some code here should be unwrapped to make shortcuts.
        var thermalConductivity = GetThermalConductivity(ref m);
        var viscosity = GetViscosity(ref m);
        return GetHeatCapacity(ref m) * viscosity / thermalConductivity;
    }

    [PublicAPI]
    public float GetPrandtlNumber<T>(ref T m, float thermalConductivity, float viscosity) where T : IGasMixture
        => GetHeatCapacity(ref m) * viscosity / MathF.Max(thermalConductivity, SystemConstants.Epsilon);
}
