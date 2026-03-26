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
    public void GetMolesRatio<T>(ref T m, Span<float> buffer) where T : IGasMixture
        => NumericsHelpers.Divide(m.Moles, GetTotalMoles(ref m), buffer);

    [PublicAPI]
    public float GetPressure<T>(ref T m) where T : IGasMixture
        => GetTotalMoles(ref m) / 1000f * m.Temperature * PhysicalConstants.R / m.Volume;

    [PublicAPI]
    public void GetPartialPressures<T>(ref T m, Span<float> buffer) where T : IGasMixture
        => GetPartialPressures(m.Moles, m.Temperature, m.Volume, buffer);

    [PublicAPI]
    public void GetPartialPressures(float[] moles, float temperature, float volume, Span<float> buffer)
        => NumericsHelpers.Multiply(moles, 0.001f * temperature * PhysicalConstants.R / volume, buffer);

    /// <summary>
    /// Gets the heat capacity of a mixture.
    /// </summary>
    /// <param name="m">The target gas mixture.</param>
    /// <returns>Calculated heat capacity.</returns>
    [PublicAPI]
    public float GetHeatCapacity<T>(ref T m) where T : IGasMixture
    {
        var buffer = Pool.Rent();
        NumericsHelpers.Multiply(m.Moles, GasSpecificHeats, buffer);
        var result = MathF.Max(NumericsHelpers.HorizontalAdd(buffer), SystemConstants.Epsilon);
        Pool.Return(buffer, true);
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
        var buffer1 = Pool.Rent();
        var buffer2 = Pool.Rent();
        GetMolesRatio(ref m, buffer1);
        NumericsHelpers.Multiply(buffer1, GasSpecificHeats, buffer2);
        var result = MathF.Max(NumericsHelpers.HorizontalAdd(buffer2), SystemConstants.Epsilon);
        Pool.Return(buffer1, true);
        Pool.Return(buffer2, true);
        return result;
    }

    /// <summary>
    /// Gets thermal conductivity of a mixture using Herding-Ciphers approximation.
    /// </summary>
    [PublicAPI]
    public float GetThermalConductivity<T>(ref T m) where T : IGasMixture
    {
        var buffer1 = Pool.Rent();
        var buffer2 = Pool.Rent();
        NumericsHelpers.Multiply(m.Moles, GasMolarMassesSquareRoots, buffer1);
        NumericsHelpers.Multiply(buffer1, GasThermalConductivities, buffer2);
        var bottomPart = NumericsHelpers.HorizontalAdd(buffer1);
        var topPart = NumericsHelpers.HorizontalAdd(buffer2);
        Pool.Return(buffer1, true);
        Pool.Return(buffer2, true);
        return topPart / MathF.Max(bottomPart, SystemConstants.Epsilon);
    }

    [PublicAPI]
    public float GetMass<T>(ref T m) where T : IGasMixture
    {
        var buffer = Pool.Rent();
        NumericsHelpers.Multiply(m.Moles, GasMolarMasses, buffer);
        var result = NumericsHelpers.HorizontalAdd(buffer) / 1000f;
        Pool.Return(buffer, true);
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
        var buffer1 = Pool.Rent();
        var buffer2 = Pool.Rent();
        NumericsHelpers.Multiply(m.Moles, GasMolarMassesSquareRoots, buffer1);
        NumericsHelpers.Multiply(buffer1, GasViscosities, buffer2);
        var bottomPart = NumericsHelpers.HorizontalAdd(buffer1);
        var topPart = NumericsHelpers.HorizontalAdd(buffer2);
        Pool.Return(buffer1, true);
        Pool.Return(buffer2, true);
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
