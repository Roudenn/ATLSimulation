using Content.Shared.Utils;
using Robust.Shared.GameStates;

namespace Content.Shared.Subgrid.Components;

/// <summary>
/// Assigned to a grid that supports a subgrid for thermodynamic simulations.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SubGridComponent : Component
{
    /// <summary>
    /// Stores chunk entities that are assigned to the center of each PVS chunk of the grid.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Vector2i, EntityUid> ChunkEntities = new();

    /// <summary>
    /// Cache for each chunk and its neighbours.
    /// </summary>
    public readonly Dictionary<Vector2i, Dictionary<Vector2i, SubGridChunk>> ChunkMapCaches = new();

    /// <summary>
    /// Cache for each chunk and its gas buffer.
    /// </summary>
    public readonly Dictionary<Vector2i, ConstantArrayPool<float>> ChunkGasBuffers = new();
}
