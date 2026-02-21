using Content.Shared.Subgrid.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Systems;

public abstract partial class SharedAtmosphericsSystem
{
    public virtual void AddHeatArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, float energy) { }

    public virtual void AdjustMolesArea(Entity<SubGridComponent?, MapGridComponent?> entity, TileRef tile, ProtoId<GasPrototype> gas, float moles) { }

    public virtual void SetVolumeArea(Entity<SubGridComponent?, MapGridComponent?> grid, TileRef tile, float volume) { }
}
