using MethodImpl = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem : EntitySystem
{
    // TODO port stuff needed from the server system to here
    [Dependency] private readonly IGasPrototypeManager _gasManager = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        InitializeGases();
    }

    public float[] GasSpecificHeats = Array.Empty<float>();
    public float[] GasMolarMasses = Array.Empty<float>();
    public float[] GasMolarMassesSquareRoots = Array.Empty<float>();
    public float[] GasViscosities = Array.Empty<float>();
    public float[] GasThermalConductivities = Array.Empty<float>();
    public float[] GasPrandtlNumbers = Array.Empty<float>();
    public float[] GasPrandtlNumbersCubicRoots = Array.Empty<float>();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InitializeGases()
    {
        Array.Resize(ref GasSpecificHeats, MathHelper.NextMultipleOf(_gasManager.Count, 4));
        Array.Resize(ref GasMolarMasses, MathHelper.NextMultipleOf(_gasManager.Count, 4));
        Array.Resize(ref GasViscosities, MathHelper.NextMultipleOf(_gasManager.Count, 4));
        Array.Resize(ref GasThermalConductivities, MathHelper.NextMultipleOf(_gasManager.Count, 4));
        Array.Resize(ref GasMolarMassesSquareRoots, MathHelper.NextMultipleOf(_gasManager.Count, 4));
        Array.Resize(ref GasPrandtlNumbers, MathHelper.NextMultipleOf(_gasManager.Count, 4));
        Array.Resize(ref GasPrandtlNumbersCubicRoots, MathHelper.NextMultipleOf(_gasManager.Count, 4));
        for (var i = 0; i < _gasManager.Count; i++)
        {
            var gas = _gasManager[i];
            
            // Resolved prototype values
            GasSpecificHeats[i] = gas.SpecificMolarHeat;
            GasMolarMasses[i] = gas.MolarMass;
            GasViscosities[i] = gas.Viscosity;
            GasThermalConductivities[i] = gas.ThermalConductivity;
            
            // Pre-calculated constant values
            GasMolarMassesSquareRoots[i] = MathF.Sqrt(gas.MolarMass);
            
            var prandtl = gas.SpecificMolarHeat * gas.Viscosity / (gas.MolarMass * gas.ThermalConductivity);
            GasPrandtlNumbers[i] = prandtl;
            GasPrandtlNumbersCubicRoots[i] = MathF.Cbrt(prandtl);
        }
    }
}
