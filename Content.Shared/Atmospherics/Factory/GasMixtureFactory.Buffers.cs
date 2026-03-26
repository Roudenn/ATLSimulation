using Content.Shared.Utils;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    public RobustArrayPool<float> Pool = default!;

    public float[] GasSpecificHeats = Array.Empty<float>();

    public float[] GasMolarMasses = Array.Empty<float>();

    public float[] GasMolarMassesSquareRoots = Array.Empty<float>();

    public float[] GasViscosities = Array.Empty<float>();

    public float[] GasThermalConductivities = Array.Empty<float>();

    /// <summary>
    /// Effective diameters of the molecules of gases that are ready
    /// for use in Fick's first law of diffusion.
    /// Formula: β = π^1.5 * d^2 * N_A * M^0.5 / 1000
    /// </summary>
    /// <remarks>
    /// Division by a thousand is required so it's possible to just
    /// multiply beta sizes with moles and get the correct result.
    /// </remarks>
    public float[] GasAtomBetaSizes = Array.Empty<float>();

    private void InitializeGases()
    {
        var size = MathHelper.NextMultipleOf(ArraySize, 4);
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
            GasAtomBetaSizes[i] = 1.5f * gas.EffectiveDiameter * MathF.PI * MathF.Sqrt(MathF.PI) * (MathF.Sqrt(gas.MolarMass / 1000f) * 6.022f);
        }
    }
}
