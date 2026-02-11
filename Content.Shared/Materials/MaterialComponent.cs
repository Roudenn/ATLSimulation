using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Materials;

[RegisterComponent, NetworkedComponent]
public sealed partial class MaterialComponent : Component
{
    [DataField(required: true)]
    public ProtoId<MaterialPrototype> Material;
}
