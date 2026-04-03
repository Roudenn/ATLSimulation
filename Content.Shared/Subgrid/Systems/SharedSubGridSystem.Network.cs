using Content.Shared.Subgrid.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    private void InitializeNetwork()
    {
        SubscribeLocalEvent<SubGridChunkComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<SubGridChunkComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnGetState(Entity<SubGridChunkComponent> ent, ref ComponentGetState args)
    {
        args.State = new SubGridChunkComponentState(GetNetEntity(ent.Comp.ParentGrid), ent.Comp.ChunkIndices, ent.Comp.ChunkData);
    }

    private void OnHandleState(Entity<SubGridChunkComponent> ent, ref ComponentHandleState args)
    {
        switch (args.Current)
        {
            case SubGridChunkComponentState fullState:
                ent.Comp.ParentGrid = GetEntity(fullState.ParentGrid);
                ent.Comp.ChunkIndices = fullState.ChunkIndices;
                ent.Comp.ChunkData = new SubGridChunk(fullState.AtmosData, fullState.HeatData);
                break;
            default:
                return;
        }
    }
}
