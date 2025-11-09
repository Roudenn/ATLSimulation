using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Client.Physics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Movement;

public sealed class MoverController : SharedMoverController
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InputMoverComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<InputMoverComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<InputMoverComponent, UpdateIsPredictedEvent>(OnUpdatePredicted);
    }

    private void OnUpdatePredicted(Entity<InputMoverComponent> entity, ref UpdateIsPredictedEvent args)
    {
        // Enable prediction if an entity is controlled by the player
        if (entity.Owner == _playerManager.LocalEntity)
            args.IsPredicted = true;
    }
    
    private void OnPlayerAttached(Entity<InputMoverComponent> entity, ref LocalPlayerAttachedEvent args)
    {
        SetMoveInput(entity, MoveButtons.None);
    }

    private void OnPlayerDetached(Entity<InputMoverComponent> entity, ref LocalPlayerDetachedEvent args)
    {
        SetMoveInput(entity, MoveButtons.None);
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        if (_playerManager.LocalEntity is not {Valid: true} player)
            return;

        HandleClientsideMovement(player, frameTime);
    }

    private void HandleClientsideMovement(EntityUid player, float frameTime)
    {
        if (!MoverQuery.TryGetComponent(player, out var mover))
        {
            return;
        }

        // Server-side should just be handled on its own so we'll just do this shizznit
        HandleMobMovement((player, mover), frameTime);
    }
}