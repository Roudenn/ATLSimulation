using Content.Server.SubGrid;
using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Atmospherics.Systems;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem : SharedAtmosphericsSystem
{
    [Dependency] private readonly SubGridSystem _subGrid = default!;

    private EntityQuery<MapGridComponent> _mapGridQuery;

    public override void Initialize()
    {
        base.Initialize();
        _mapGridQuery = GetEntityQuery<MapGridComponent>();
    }

    public override void AddHeatArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, float energy)
    {
        if (!_mapGridQuery.Resolve(grid.Owner, ref grid.Comp2)
            || !_subGrid.TryGetChunk(grid, tile, out var chunk))
            return;

        var indexes = _subGrid.GetAreaTileIndexesAtTile(tile.GridIndices, grid.Comp2.TileSizeVector);
        energy /= indexes.Length;

        foreach (var index in indexes)
        {
            AddHeat(ref chunk.Value.Comp.AtmosphereMap[index].Mixture, energy);
        }
    }

    public override void AdjustMolesArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, ProtoId<GasPrototype> gas, float moles)
    {
        if (!_mapGridQuery.Resolve(grid.Owner, ref grid.Comp2)
            || !_subGrid.TryGetChunk(grid, tile, out var chunk))
            return;

        var indexes = _subGrid.GetAreaTileIndexesAtTile(tile.GridIndices, grid.Comp2.TileSizeVector);
        moles /= indexes.Length;

        foreach (var index in indexes)
        {
            AdjustMoles(ref chunk.Value.Comp.AtmosphereMap[index].Mixture, gas, moles);
        }
    }

    public override void SetVolumeArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, float volume)
    {
        if (!_mapGridQuery.Resolve(grid.Owner, ref grid.Comp2)
            || !_subGrid.TryGetChunk(grid, tile, out var chunk))
            return;

        var indexes = _subGrid.GetAreaTileIndexesAtTile(tile.GridIndices, grid.Comp2.TileSizeVector);
        foreach (var index in indexes)
        {
            chunk.Value.Comp.AtmosphereMap[index].Mixture.SetVolume(volume);
        }
    }
}
