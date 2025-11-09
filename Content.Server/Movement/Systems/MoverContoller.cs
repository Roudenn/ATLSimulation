using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Player;

namespace Content.Server.Movement.Systems;

public sealed class MoverController : SharedMoverController
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InputMoverComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<InputMoverComponent, PlayerDetachedEvent>(OnPlayerDetached);
    }
    
    private void OnPlayerAttached(Entity<InputMoverComponent> entity, ref PlayerAttachedEvent args)
    {
        SetMoveInput(entity, MoveButtons.None);
    }

    private void OnPlayerDetached(Entity<InputMoverComponent> entity, ref PlayerDetachedEvent args)
    {
        SetMoveInput(entity, MoveButtons.None);
    }
    
    private HashSet<EntityUid> _moverAdded = new();
    private List<Entity<InputMoverComponent>> _movers = new();

    private void InsertMover(Entity<InputMoverComponent> source)
    {
        // Already added
        if (!_moverAdded.Add(source.Owner))
            return;

        _movers.Add(source);
    }
    
    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        _moverAdded.Clear();
        _movers.Clear();
        var inputQueryEnumerator = AllEntityQuery<InputMoverComponent>();

        // Need to order mob movement so that movers don't run before their relays.
        while (inputQueryEnumerator.MoveNext(out var uid, out var mover))
        {
            InsertMover((uid, mover));
        }

        foreach (var mover in _movers)
        {
            HandleMobMovement(mover, frameTime);
        }
    }
}
