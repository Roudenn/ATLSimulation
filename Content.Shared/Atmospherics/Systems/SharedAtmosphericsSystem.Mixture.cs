using Content.Shared.Atmospherics.GasMixtures;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Systems;

// This part contains simplified API for interacting with gas mixture helpers
public abstract partial class SharedAtmosphericsSystem
{
    [PublicAPI]
    public float GetMoles(ref GasMixture m, ProtoId<GasPrototype> proto)
    {
        var gasId = _protoMan.Index(proto).GasId;
        return m.GetMoles(gasId);
    }
    
    
    
    [PublicAPI]
    public float GetHeatCapacity(ref GasMixture m)
        => m.GetHeatCapacityQuery(ref GasBuffer, ref GasSpecificHeats);
    
    [PublicAPI]
    public float GetSpecificHeatCapacity(ref GasMixture m)
        => m.GetSpecificHeatCapacityQuery(ref GasBuffer, ref GasSpecificHeats);
        
    [PublicAPI]
    public float GetMass(ref GasMixture m)
        => m.GetMassQuery(ref GasBuffer, ref GasMolarMasses);
        
    [PublicAPI]
    public float GetDensity(ref GasMixture m)
        => m.GetDensityQuery(ref GasBuffer, ref GasMolarMasses);
    
    [PublicAPI]
    public float GetThermalConductivity(ref GasMixture m)
        => m.GetThermalConductivityQuery(ref GasBuffer, ref GasBuffer2, ref GasMolarMassesSquareRoots, ref GasThermalConductivities);
        
    [PublicAPI]
    public float GetViscosity(ref GasMixture m)
        => m.GetViscosityQuery(ref GasBuffer, ref GasBuffer2, ref GasMolarMassesSquareRoots, ref GasViscosities);
}
