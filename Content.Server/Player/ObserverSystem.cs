using Content.Shared.Player;
using Robust.Shared.Player;

namespace Content.Server.Player;

public sealed class ObserverSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ObserverComponent, PlayerDetachedEvent>(OnDetached);
    }

    private void OnDetached(Entity<ObserverComponent> ent, ref PlayerDetachedEvent args)
    {
        QueueDel(ent.Owner);
    }
}
