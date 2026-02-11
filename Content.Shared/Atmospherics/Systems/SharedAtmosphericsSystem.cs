using Robust.Shared.Prototypes;
using MethodImpl = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem : EntitySystem
{
    // TODO port stuff needed from the server system to here
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IGasPrototypeManager _gasManager = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        InitializeGases();
    }

    /// <summary>
    /// A buffer used for temporarily storing data while using NumericsHelpers.
    /// Always has the size equal to <see cref="GasPrototypeManager.ArraySize"/>
    /// </summary>
    public float[] GasBuffer = Array.Empty<float>();
    public float[] GasBuffer2 = Array.Empty<float>();
    
    public float[] GasSpecificHeats = Array.Empty<float>();
    public float[] GasMolarMasses = Array.Empty<float>();
    public float[] GasMolarMassesSquareRoots = Array.Empty<float>();
    public float[] GasViscosities = Array.Empty<float>();
    public float[] GasThermalConductivities = Array.Empty<float>();
    public float[] GasPrandtlNumbersCubicRoots = Array.Empty<float>();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            
            // Pre-calculated constant values
            GasMolarMassesSquareRoots[i] = MathF.Sqrt(gas.MolarMass);
            GasPrandtlNumbersCubicRoots[i] = MathF.Cbrt(gas.PrandtlNumber);
        }
    }
}
