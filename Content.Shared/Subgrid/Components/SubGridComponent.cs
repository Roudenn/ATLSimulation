using Robust.Shared.GameStates;

namespace Content.Shared.Subgrid.Components;

/// <summary>
/// Assigned to a grid that supports a subgrid for thermodynamic simulations.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SubGridComponent : Component
{
    /// <summary>
    /// Stores chunk entities that are assigned to the center of each PVS chunk of the grid.
    /// </summary>
    [ViewVariables]
    public Dictionary<Vector2i, EntityUid> ChunkEntities = new();
}
