using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Subgrid.Chunks;
using Content.Shared.Temperature.HeatContainers;
using Content.Shared.Utils;
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

    /// <summary>
    /// Chunk buffer used for storing references to neighbouring chunks.
    /// </summary>
    public Dictionary<Vector2i, SubGridChunk> ChunkBuffer = new();

    /// <summary>
    /// Array pool used for <see cref="GasMixtureFactory"/> calculations.
    /// </summary>
    public ConstantArrayPool<float> GasArrayPool;

    /// <summary>
    /// A buffer used when serializing the full state on server side.
    /// </summary>
    public GasMixture[] AtmosBuffer = new GasMixture[0];

    /// <summary>
    /// A buffer used when serializing the full state on server side.
    /// </summary>
    public ConductiveHeatContainer[] HeatBuffer = new ConductiveHeatContainer[0];
}

[Serializable, NetSerializable]
public sealed partial class SubGridChunkComponentState : ComponentState
{
    public NetEntity ParentGrid;
    public Vector2i ChunkIndices;
    public GasMixture[] AtmosData;
    public ConductiveHeatContainer[] HeatData;

    public SubGridChunkComponentState(NetEntity parentGrid, Vector2i chunkIndices, GasMixture[] atmosData, ConductiveHeatContainer[] heatData)
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
        var atmos = new GasMixture[fullState.AtmosData.Length];
        fullState.AtmosData.AsSpan().CopyTo(atmos);
        var heat = new ConductiveHeatContainer[fullState.HeatData.Length];
        fullState.HeatData.AsSpan().CopyTo(heat);
        return new SubGridChunkComponentState(fullState.ParentGrid, fullState.ChunkIndices, atmos, heat);
    }
}
