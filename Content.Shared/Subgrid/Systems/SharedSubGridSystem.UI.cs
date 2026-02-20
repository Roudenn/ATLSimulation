using Content.Shared.Player;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    private void InitializeUI()
    {
        SubscribeLocalEvent<ObserverComponent, SubGridAddHeatMessage>(OnAddHeat);
        SubscribeLocalEvent<ObserverComponent, SubGridAddMolesMessage>(OnAddMoles);
        SubscribeLocalEvent<ObserverComponent, SubGridSetVolumeMessage>(OnSetVolume);
    }

    private void OnAddHeat(Entity<ObserverComponent> ent, ref SubGridAddHeatMessage args)
    {

    }

    private void OnAddMoles(Entity<ObserverComponent> ent, ref SubGridAddMolesMessage args)
    {

    }

    private void OnSetVolume(Entity<ObserverComponent> ent, ref SubGridSetVolumeMessage args)
    {

    }
}
