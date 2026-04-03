using System.Linq;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Temperature.HeatContainers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Subgrid.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SubGridChunkComponent : Component
{
    // TODO: remove this shitcode when a method to parent stuff to grids even when they are off grid gets added
    // or the entire chunk system for PVS is reworked that could also work
    [DataField]
    public EntityUid ParentGrid;

    [DataField]
    public Vector2i ChunkIndices;

    [DataField]
    public SubGridChunk ChunkData;
}

[Serializable, NetSerializable]
public sealed partial class SubGridChunkComponentState : ComponentState
{
    public NetEntity ParentGrid;
    public Vector2i ChunkIndices;
    public VelocityGasMixture[] AtmosData;
    public ConductiveHeatContainer[] HeatData;

    public SubGridChunkComponentState(NetEntity parentGrid, Vector2i chunkIndices, SubGridChunk chunkData)
    {
        ParentGrid = parentGrid;
        ChunkIndices = chunkIndices;
        AtmosData = chunkData.AtmosphereMap.Where(x => x.Initialized).Select(x => x.ArchivedMixture).ToArray();
        HeatData = chunkData.TemperatureMap.Where(x => x.Initialized).Select(x => x.ArchivedContainer).ToArray();
    }

    public SubGridChunkComponentState(NetEntity parentGrid, Vector2i chunkIndices, VelocityGasMixture[] atmosData, ConductiveHeatContainer[] heatData)
    {
        ParentGrid = parentGrid;
        ChunkIndices = chunkIndices;
        AtmosData = atmosData;
        HeatData = heatData;
    }
}

[Serializable, NetSerializable]
public sealed partial class SubGridChunkComponentDeltaState(NetEntity parentGrid, Vector2i chunkIndices) : ComponentState, IComponentDeltaState<SubGridChunkComponentState>
{
    public NetEntity ParentGrid = parentGrid;
    public Vector2i ChunkIndices = chunkIndices;

    public void ApplyToFullState(SubGridChunkComponentState fullState)
    {

    }

    public SubGridChunkComponentState CreateNewFullState(SubGridChunkComponentState fullState)
    {
        var atmos = new VelocityGasMixture[fullState.AtmosData.Length];
        fullState.AtmosData.AsSpan().CopyTo(atmos);
        var heat = new ConductiveHeatContainer[fullState.HeatData.Length];
        fullState.HeatData.AsSpan().CopyTo(heat);
        return new SubGridChunkComponentState(fullState.ParentGrid, fullState.ChunkIndices, atmos, heat);
    }
}
