using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Maps;

/// <summary>
/// Represents a selectable simulation map.
/// </summary>
[Prototype]
public sealed partial class SimulationMapPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ViewVariables]
    public LocId Name => $"map-{ID}";

    [ViewVariables]
    public LocId Description => $"map-{ID}-desc";

    [DataField(required: true)]
    public ResPath? Path;
}
