using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Subgrid.Chunks;
using Content.Shared.Subgrid.Components;
using Content.Shared.Temperature.HeatContainers;
using Robust.Shared.GameStates;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    private void InitializeNetwork()
    {
        SubscribeLocalEvent<SubGridChunkComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(Entity<SubGridChunkComponent> ent, ref ComponentHandleState args)
    {
        switch (args.Current)
        {
            case SubGridChunkComponentState fullState:
                ent.Comp.ParentGrid = GetEntity(fullState.ParentGrid);
                ent.Comp.ChunkIndices = fullState.ChunkIndices;
                ent.Comp.ChunkData ??= new SubGridChunk(SubGridChunkSize); // TODO I don't know how to initialize this properly beforehand...
                ent.Comp.ChunkData.ApplyState(fullState.AtmosData, fullState.HeatData);
                break;
            default:
                return;
        }
    }
}
