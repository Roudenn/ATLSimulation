namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem
{
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
        Array.Resize(ref GasBuffer, _gasManager.ArraySize);
        Array.Resize(ref GasBuffer2, _gasManager.ArraySize);
        Array.Resize(ref GasSpecificHeats, _gasManager.ArraySize);
        Array.Resize(ref GasMolarMasses, _gasManager.ArraySize);
        Array.Resize(ref GasViscosities, _gasManager.ArraySize);
        Array.Resize(ref GasThermalConductivities, _gasManager.ArraySize);
        Array.Resize(ref GasMolarMassesSquareRoots, _gasManager.ArraySize);
        Array.Resize(ref GasPrandtlNumbersCubicRoots, _gasManager.ArraySize);
        for (var i = 0; i < _gasManager.Count; i++)
        {
            var gas = _gasManager[i];

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
