using Content.Shared.Subgrid.Chunks;
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
        for (var index = 0; index < ent.Comp.HeatBuffer.Length; index++)
        {
            ent.Comp.HeatBuffer[index].Initialized = false;
        }

        for (var index = 0; index < ent.Comp.AtmosBuffer.Length; index++)
        {
            ent.Comp.AtmosBuffer[index].Initialized = false;
        }

        var atmosMap = ent.Comp.ChunkData.AtmosphereMap;
        for (int i = 0; i < atmosMap.Length; i++)
        {
            if (!atmosMap[i].Initialized)
                continue;

            ent.Comp.AtmosBuffer[i] = atmosMap[i].Mixture;
        }

        var tempMap = ent.Comp.ChunkData.TemperatureMap;
        for (int i = 0; i < tempMap.Length; i++)
        {
            if (!tempMap[i].Initialized)
                continue;

            ent.Comp.HeatBuffer[i] = tempMap[i].Container;
        }

        args.State = new SubGridChunkComponentState(GetNetEntity(ent.Comp.ParentGrid), ent.Comp.ChunkIndices, ent.Comp.AtmosBuffer, ent.Comp.HeatBuffer);
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
