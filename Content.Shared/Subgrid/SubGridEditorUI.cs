using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Temperature.HeatContainers;
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

[ByRefEvent]
public record struct InspectSubGridAtmosphereTile(
    GasMixture? GasMixture,
    EntityUid Grid,
    Vector2i ChunkIndices,
    int Index);

[ByRefEvent]
public record struct InspectSubGridHeatTile(
    ConductiveHeatContainer? HeatContainer,
    EntityUid Grid,
    Vector2i ChunkIndices,
    int Index);
