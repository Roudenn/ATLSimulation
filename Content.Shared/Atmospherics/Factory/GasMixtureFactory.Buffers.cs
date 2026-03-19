using Robust.Shared.Utility;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    /// <summary>
    /// A buffer used for temporarily storing data while using NumericsHelpers.
    /// Always has the size equal to <see cref="GasMixtureFactory.ArraySize"/>
    /// </summary>
    /// <remarks>
    /// BEWARE OF BUFFER COLLISIONS!
    /// If you use a bugger in a function, then call some other method,
    /// and it also tries to use the same buffer - everything will break!
    /// Be careful with managing the buffers to prevent such memory catastrophes.
    /// </remarks>
    public float[] GasBuffer1 = Array.Empty<float>();

    /// <inheritdoc cref="GasBuffer1"/>
    public float[] GasBuffer2 = Array.Empty<float>();

    /// <inheritdoc cref="GasBuffer1"/>
    public float[] GasBuffer3 = Array.Empty<float>();

    /// <inheritdoc cref="GasBuffer1"/>
    public float[] GasBuffer4 = Array.Empty<float>();

    /// <summary>
    /// Buffer that is generally used to store results of a function that returns an array of gases.
    /// This helps to prevent unnecessary memory allocations and buffer collisions.
    /// </summary>
    public float[] GasBufferResults1 = Array.Empty<float>();

    /// <inheritdoc cref="GasBufferResults1"/>
    public float[] GasBufferResults2 = Array.Empty<float>();

    public float[] GasSpecificHeats = Array.Empty<float>();

    public float[] GasMolarMasses = Array.Empty<float>();

    public float[] GasMolarMassesSquareRoots = Array.Empty<float>();

    public float[] GasViscosities = Array.Empty<float>();

    public float[] GasThermalConductivities = Array.Empty<float>();

    /// <summary>
    /// Effective diameters of the molecules of gases that are ready
    /// for use in Fick's first law of diffusion.
    /// Formula: β = π^1.5 * d^2 * N_A * M^0.5
    /// </summary>
    public float[] GasAtomBetaSizes = Array.Empty<float>();

    private void InitializeGases()
    {
        var size = MathHelper.NextMultipleOf(ArraySize, 4);
        Array.Resize(ref GasBuffer1, size);
        Array.Resize(ref GasBuffer2, size);
        Array.Resize(ref GasBuffer3, size);
        Array.Resize(ref GasBuffer4, size);
        Array.Resize(ref GasBufferResults1, size);
        Array.Resize(ref GasBufferResults2, size);
        Array.Resize(ref GasSpecificHeats, size);
        Array.Resize(ref GasMolarMasses, size);
        Array.Resize(ref GasViscosities, size);
        Array.Resize(ref GasThermalConductivities, size);
        Array.Resize(ref GasMolarMassesSquareRoots, size);
        Array.Resize(ref GasAtomBetaSizes, size);

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
            GasAtomBetaSizes[i] = 1.5f * gas.EffectiveDiameter * MathF.PI * MathF.Sqrt(MathF.PI) * (MathF.Sqrt(gas.MolarMass / 1000f) * 6022f);
        }
    }

    private void ClearBuffer(ref float[] buffer)
    {
        NumericsHelpers.Min(buffer, 0f);
        NumericsHelpers.Max(buffer, 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer) == 0f);
    }

    private void ClearBuffer(ref float[] buffer1, ref float[] buffer2)
    {
        NumericsHelpers.Min(buffer1, 0f);
        NumericsHelpers.Min(buffer2, 0f);
        NumericsHelpers.Max(buffer1, 0f);
        NumericsHelpers.Max(buffer2, 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer1) == 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer2) == 0f);
    }

    private void ClearBuffer(ref float[] buffer1, ref float[] buffer2, ref float[] buffer3)
    {
        NumericsHelpers.Min(buffer1, 0f);
        NumericsHelpers.Min(buffer2, 0f);
        NumericsHelpers.Min(buffer3, 0f);
        NumericsHelpers.Max(buffer1, 0f);
        NumericsHelpers.Max(buffer2, 0f);
        NumericsHelpers.Max(buffer3, 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer1) == 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer2) == 0f);
        DebugTools.Assert(NumericsHelpers.HorizontalAdd(buffer3) == 0f);
    }
}
