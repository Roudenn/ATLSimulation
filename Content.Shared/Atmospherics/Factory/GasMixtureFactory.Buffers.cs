using Robust.Shared.Utility;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    /// <summary>
    /// A buffer used for temporarily storing data while using NumericsHelpers.
    /// Always has the size equal to <see cref="GasMixtureFactory.ArraySize"/>
    /// </summary>
    public float[] GasBuffer = Array.Empty<float>();

    /// <inheritdoc cref="GasBuffer"/>
    public float[] GasBuffer2 = Array.Empty<float>();

    public float[] GasBufferRatios = Array.Empty<float>();

    public float[] GasSpecificHeats = Array.Empty<float>();

    public float[] GasMolarMasses = Array.Empty<float>();

    public float[] GasMolarMassesSquareRoots = Array.Empty<float>();

    public float[] GasViscosities = Array.Empty<float>();

    public float[] GasThermalConductivities = Array.Empty<float>();

    public float[] GasPrandtlNumbers = Array.Empty<float>();

    private void InitializeGases()
    {
        var size = MathHelper.NextMultipleOf(ArraySize, 4);
        Array.Resize(ref GasBuffer, size);
        Array.Resize(ref GasBuffer2, size);
        Array.Resize(ref GasBufferRatios, size);
        Array.Resize(ref GasSpecificHeats, size);
        Array.Resize(ref GasMolarMasses, size);
        Array.Resize(ref GasViscosities, size);
        Array.Resize(ref GasThermalConductivities, size);
        Array.Resize(ref GasMolarMassesSquareRoots, size);
        Array.Resize(ref GasPrandtlNumbers, size);

        for (var i = 0; i < Count; i++)
        {
            var gas = this[i];

            // Resolved prototype values
            GasSpecificHeats[i] = gas.SpecificMolarHeat;
            GasMolarMasses[i] = gas.MolarMass;
            GasViscosities[i] = gas.Viscosity;
            GasThermalConductivities[i] = gas.ThermalConductivity;

            // Pre-calculated values
            GasMolarMassesSquareRoots[i] = MathF.Sqrt(gas.MolarMass / 1000f);
        }
    }

    private void ClearBuffer(ref float[] buffer)
    {
        NumericsHelpers.Min(buffer, 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer) == 0f);
    }

    private void ClearBuffer(ref float[] buffer1, ref float[] buffer2)
    {
        NumericsHelpers.Min(buffer1, 0f);
        NumericsHelpers.Min(buffer2, 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer1) == 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer2) == 0f);
    }

    private void ClearBuffer(ref float[] buffer1, ref float[] buffer2, ref float[] buffer3)
    {
        NumericsHelpers.Min(buffer1, 0f);
        NumericsHelpers.Min(buffer2, 0f);
        NumericsHelpers.Min(buffer3, 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer1) == 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer2) == 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer3) == 0f);
    }
}
