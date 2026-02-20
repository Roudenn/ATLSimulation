using Content.Shared.Atmospherics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Subgrid;

[Serializable, NetSerializable]
public sealed class SubGridAddHeatMessage(NetEntity targetGrid, Vector2i tileIndices, float energy) : BoundUserInterfaceMessage
{
    public NetEntity TargetGrid { get; } = targetGrid;
    public Vector2i TileIndices { get; } = tileIndices;
    public float Energy { get; } = energy;
}

[Serializable, NetSerializable]
public sealed class SubGridAddMolesMessage(
    NetEntity targetGrid,
    Vector2i tileIndices,
    ProtoId<GasPrototype> gas,
    float moles) : BoundUserInterfaceMessage
{
    public NetEntity TargetGrid { get; } = targetGrid;
    public Vector2i TileIndices { get; } = tileIndices;
    public ProtoId<GasPrototype> Gas { get; } = gas;
    public float Moles { get; } = moles;
}

[Serializable, NetSerializable]
public sealed class SubGridSetVolumeMessage(NetEntity targetGrid, Vector2i tileIndices, float volume) : BoundUserInterfaceMessage
{
    public NetEntity TargetGrid { get; } = targetGrid;
    public Vector2i TileIndices { get; } = tileIndices;
    public float Volume { get; } = volume;
}
