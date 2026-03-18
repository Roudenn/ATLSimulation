using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.GasMixtures;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Prototypes;

[ImplicitDataDefinitionForInheritors]
public abstract partial class MixtureDefinition
{
    public abstract IGasMixture CreateMixture(IGasMixtureFactory factory, IPrototypeManager protoMan, float? volume = null);
}
