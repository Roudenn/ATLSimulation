using Content.Shared.Atmospherics.Components;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Subgrid.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] protected readonly GasMixtureFactory GasManager = default!;
    [Dependency] private readonly SharedSubGridSystem _subGrid = default!;

    private EntityQuery<GridAtmosphereComponent> _atmosGridQuery;

    public override void Initialize()
    {
        base.Initialize();
        InitializeCVars();

        _atmosGridQuery = GetEntityQuery<GridAtmosphereComponent>();
    }

    public GasMixEntry GenerateGaxMixEntry<T>(T m) where T : IGasMixture
    {
        var gases = new GasEntry[GasManager.ArraySize];
        for (var index = 0; index < m.Moles.Length; index++)
        {
            var gasAmount = m.Moles[index];
            var gasProto = GasManager[index];
            gases[index] = new GasEntry(gasProto.Name, gasAmount, gasProto.Color);
        }

        return new GasMixEntry(
            GasManager.GetTotalMoles(ref m),
            m.Volume,
            GasManager.GetPressure(ref m),
            m.Temperature,
            GasManager.GetHeatCapacity(ref m),
            GasManager.GetThermalConductivity(ref m),
            GasManager.GetViscosity(ref m),
            GasManager.GetMass(ref m),
            GasManager.GetPrandtlNumber(ref m),
            gases);
    }
}
