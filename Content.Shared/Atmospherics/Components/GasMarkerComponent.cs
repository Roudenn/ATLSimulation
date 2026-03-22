using Content.Shared.Atmospherics.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Components;

[RegisterComponent]
public sealed partial class GasMarkerComponent : Component
{
    [DataField(required: true)]
    public ProtoId<GasMixturePrototype> Mixture;
}
