namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    private void InitializeUI()
    {
        SubscribeNetworkEvent<SubGridAddHeatMessage>(OnAddHeat);
        SubscribeNetworkEvent<SubGridAddMolesMessage>(OnAddMoles);
    }

    private void OnAddHeat(SubGridAddHeatMessage args)
    {
        var grid = GetEntity(args.TargetGrid);
        if (!MapGridQuery.TryComp(grid, out var gridComp)
            || !SubGridQuery.TryComp(grid, out var subGridComp))
            return;

        var tile = MapSystem.GetTileRef(grid, gridComp, args.TileIndices);
        _atmospherics.AddHeatArea((grid, subGridComp, gridComp), tile, args.Energy);
        _temperature.AddHeatArea((grid, subGridComp, gridComp), tile, args.Energy);
    }

    private void OnAddMoles(SubGridAddMolesMessage args)
    {
        var grid = GetEntity(args.TargetGrid);
        if (!MapGridQuery.TryComp(grid, out var gridComp)
            || !SubGridQuery.TryComp(grid, out var subGridComp))
            return;

        var tile = MapSystem.GetTileRef(grid, gridComp, args.TileIndices);
        _atmospherics.AdjustMolesArea((grid, subGridComp, gridComp), tile, args.Gas, args.Moles);
    }
}
