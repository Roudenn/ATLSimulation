using Content.Shared.Atmospherics.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Components;

/// <summary>
/// Component that is used when initializing grid's atmos to apply the same atmos on all solid tiles,
/// with an exception to tiles with entity markers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GridAtmosphereComponent : Component
{
    [DataField(required: true)]
    public ProtoId<GasMixturePrototype> Mixture;
}
