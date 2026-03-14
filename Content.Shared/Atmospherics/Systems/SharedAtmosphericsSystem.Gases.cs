namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem
{
    // TODO I smell that this is bad but there is no easy solutions for now sadly
    /// <summary>
    /// A buffer used for temporarily storing data while using NumericsHelpers.
    /// Always has the size equal to <see cref="GasPrototypeManager.ArraySize"/>
    /// </summary>
    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasBuffer = Array.Empty<float>();

    /// <inheritdoc cref="GasBuffer"/>
    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasBuffer2 = Array.Empty<float>();

    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasSpecificHeats = Array.Empty<float>();

    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasMolarMasses = Array.Empty<float>();

    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasMolarMassesSquareRoots = Array.Empty<float>();

    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasViscosities = Array.Empty<float>();

    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasThermalConductivities = Array.Empty<float>();

    [Access(typeof(SharedAtmosphericsSystem))]
    public float[] GasPrandtlNumbersCubicRoots = Array.Empty<float>();

    private void InitializeGases()
    {
        Array.Resize(ref GasBuffer, GasManager.ArraySize);
        Array.Resize(ref GasBuffer2, GasManager.ArraySize);
        Array.Resize(ref GasSpecificHeats, GasManager.ArraySize);
        Array.Resize(ref GasMolarMasses, GasManager.ArraySize);
        Array.Resize(ref GasViscosities, GasManager.ArraySize);
        Array.Resize(ref GasThermalConductivities, GasManager.ArraySize);
        Array.Resize(ref GasMolarMassesSquareRoots, GasManager.ArraySize);
        Array.Resize(ref GasPrandtlNumbersCubicRoots, GasManager.ArraySize);
        for (var i = 0; i < GasManager.Count; i++)
        {
            var gas = GasManager[i];

            // Resolved prototype values
            GasSpecificHeats[i] = gas.SpecificMolarHeat;
            GasMolarMasses[i] = gas.MolarMass;
            GasViscosities[i] = gas.Viscosity;
            GasThermalConductivities[i] = gas.ThermalConductivity;

            // Pre-calculated values
            GasMolarMassesSquareRoots[i] = MathF.Sqrt(gas.MolarMass);
            GasPrandtlNumbersCubicRoots[i] = MathF.Cbrt(gas.PrandtlNumber);
        }
    }
}
