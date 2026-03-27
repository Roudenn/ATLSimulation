namespace Content.Shared.Atmospherics;

public sealed class GasPrototypesBuffer
{
    public readonly float[] GasSpecificHeats = Array.Empty<float>();

    public readonly float[] GasMolarMasses = Array.Empty<float>();

    public readonly float[] GasViscosities = Array.Empty<float>();

    public readonly float[] GasThermalConductivities = Array.Empty<float>();

    public readonly float[] GasMolarMassesSquareRoots = Array.Empty<float>();

    /// <summary>
    /// Effective diameters of the molecules of gases that are ready
    /// for use in Fick's first law of diffusion.
    /// Formula: β = π^1.5 * d^2 * N_A * M^0.5 / 1000
    /// </summary>
    /// <remarks>
    /// Division by a thousand is required so it's possible to just
    /// multiply beta sizes with moles and get the correct result.
    /// </remarks>
    public readonly float[] GasAtomBetaSizes = Array.Empty<float>();

    public GasPrototypesBuffer(int arraySize)
    {
        Array.Resize(ref GasSpecificHeats, arraySize);
        Array.Resize(ref GasMolarMasses, arraySize);
        Array.Resize(ref GasViscosities, arraySize);
        Array.Resize(ref GasThermalConductivities, arraySize);
        Array.Resize(ref GasMolarMassesSquareRoots, arraySize);
        Array.Resize(ref GasAtomBetaSizes, arraySize);
    }

    public void RegisterPrototype(GasPrototype gas)
    {
        GasSpecificHeats[gas.GasId] = gas.SpecificMolarHeat;
        GasMolarMasses[gas.GasId] = gas.MolarMass;
        GasViscosities[gas.GasId] = gas.Viscosity;
        GasThermalConductivities[gas.GasId] = gas.ThermalConductivity;

        // Pre-calculated values
        GasMolarMassesSquareRoots[gas.GasId] = MathF.Sqrt(gas.MolarMass / 1000f);
        GasAtomBetaSizes[gas.GasId] = 1.5f * gas.EffectiveDiameter * MathF.PI * MathF.Sqrt(MathF.PI) * (MathF.Sqrt(gas.MolarMass / 1000f) * 6.022f);
    }
}
