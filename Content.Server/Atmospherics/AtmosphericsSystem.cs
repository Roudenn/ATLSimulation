using Content.Server.SubGrid;
using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Systems;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Threading;
using Robust.Shared.Timing;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem : SharedAtmosphericsSystem
{
    [Dependency] private readonly SubGridSystem _subGrid = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;

    private EntityQuery<MapGridComponent> _mapGridQuery;
    private EntityQuery<SubGridChunkComponent> _subgridChunkQuery;

    public override void Initialize()
    {
        base.Initialize();
        InitializeUpdate();
        _mapGridQuery = GetEntityQuery<MapGridComponent>();
        _subgridChunkQuery = GetEntityQuery<SubGridChunkComponent>();
    }

    public override void AddHeatArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, float energy)
    {
        if (!_mapGridQuery.Resolve(grid.Owner, ref grid.Comp2)
            || !_subGrid.TryGetChunk(grid, tile, out var chunk))
            return;

        var indexes = _subGrid.GetAreaTileIndexesAtTile(chunk.Value.Comp.ChunkIndices, tile.GridIndices, grid.Comp2.TileSizeVector);
        energy /= indexes.Length;

        foreach (var index in indexes)
        {
            GasManager.AddHeat(ref chunk.Value.Comp.ChunkData.AtmosphereMap[index].Mixture, energy);
        }
    }

    public override void AdjustMolesArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, ProtoId<GasPrototype> gas, float moles)
    {
        if (!_mapGridQuery.Resolve(grid.Owner, ref grid.Comp2)
            || !_subGrid.TryGetChunk(grid, tile, out var chunk))
            return;

        var indexes = _subGrid.GetAreaTileIndexesAtTile(chunk.Value.Comp.ChunkIndices, tile.GridIndices, grid.Comp2.TileSizeVector);
        moles /= indexes.Length;

        foreach (var index in indexes)
        {
            GasManager.AddMoles(ref chunk.Value.Comp.ChunkData.AtmosphereMap[index].Mixture, gas, moles);
        }
    }
}
