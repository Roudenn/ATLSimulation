using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using Content.Shared.Temperature.HeatContainers;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Atmospherics.Systems;

// This part contains simplified API for interacting with gas mixture helpers
public abstract partial class SharedAtmosphericsSystem
{
    [PublicAPI]
    public GasMixture GetGridMixture(EntityUid grid)
    {
        var mixture = _atmosGridQuery.CompOrNull(grid)?.Mixture ?? GetSpaceMixture();
        DebugTools.Assert(mixture.Immutable);
        return mixture;
    }

    [PublicAPI]
    public GasMixture GetSpaceMixture()
    {
        return new GasMixture(_gasManager, _subGrid.SubGridTileVolume, PhysicalConstants.TCMB, true);
    }

    [PublicAPI]
    public void AddHeat(ref GasMixture m, float amount)
    {
        m.AddHeat(amount);
    }

    [PublicAPI]
    public float AdjustMoles(ref GasMixture m, ProtoId<GasPrototype> proto, float moles)
    {
        var gasId = _protoMan.Index(proto).GasId;
        return m.AdjustMoles(gasId, moles);
    }

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

    /// <summary>
    /// Creates a heat container based on a gas mixture with given temperature.
    /// </summary>
    [PublicAPI]
    public HeatContainer CreateGasHeatContainer(ref GasMixture m, float temperature = PhysicalConstants.ROOM_TEMPERATURE)
    {
        return new HeatContainer(GetHeatCapacity(ref m), temperature, GetThermalConductivity(ref m));
    }
}
