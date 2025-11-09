using System.Numerics;
using Content.Shared.Movement.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.Movement.Systems;

/// <summary>
///     Handles player and NPC mob movement.
///     NPCs are handled server-side only.
/// </summary>
public abstract partial class SharedMoverController : VirtualController
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private   readonly SharedTransformSystem _transform = default!;
    
    protected EntityQuery<InputMoverComponent> MoverQuery;
    protected EntityQuery<MapComponent> MapQuery;
    protected EntityQuery<MapGridComponent> MapGridQuery;
    protected EntityQuery<MovementSpeedModifierComponent> ModifierQuery;
    protected EntityQuery<PhysicsComponent> PhysicsQuery;
    protected EntityQuery<TransformComponent> XformQuery;
    
    public override void Initialize()
    {
        base.Initialize();

        MoverQuery = GetEntityQuery<InputMoverComponent>();
        ModifierQuery = GetEntityQuery<MovementSpeedModifierComponent>();
        PhysicsQuery = GetEntityQuery<PhysicsComponent>();
        XformQuery = GetEntityQuery<TransformComponent>();
        MapGridQuery = GetEntityQuery<MapGridComponent>();
        MapQuery = GetEntityQuery<MapComponent>();
        
        InitializeInput();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        ShutdownInput();
    }

    /// <summary>
    ///     Movement while considering actionblockers, weightlessness, etc.
    /// </summary>
    protected void HandleMobMovement(
        Entity<InputMoverComponent> entity,
        float frameTime)
    {
        var uid = entity.Owner;
        var mover = entity.Comp;

        if (!XformQuery.TryComp(entity.Owner, out var xform))
            return;
        
        // Update relative movement
        if (mover.LerpTarget < Timing.CurTime)
        {
            TryUpdateRelative(uid, mover, xform);
        }

        LerpRotation(uid, mover, frameTime);

        // If we can't move then just use tile-friction / no movement handling.
        if (!mover.CanMove
            || !PhysicsQuery.TryComp(uid, out var physicsComponent))
        {
            return;
        }

        var moveSpeedComponent = ModifierQuery.CompOrNull(uid);

        float friction;
        float accel;
        Vector2 wishDir;
        var velocity = physicsComponent.LinearVelocity;
        
        var walkSpeed = moveSpeedComponent?.CurrentWalkSpeed ?? MovementSpeedModifierComponent.DefaultBaseWalkSpeed;
        var sprintSpeed = moveSpeedComponent?.CurrentSprintSpeed ?? MovementSpeedModifierComponent.DefaultBaseSprintSpeed;

        wishDir = AssertValidWish(mover, walkSpeed, sprintSpeed);

        if (wishDir != Vector2.Zero)
        {
            friction = moveSpeedComponent?.Friction ?? MovementSpeedModifierComponent.DefaultFriction;
        }
        else
        {
            friction = moveSpeedComponent?.FrictionNoInput ?? MovementSpeedModifierComponent.DefaultFrictionNoInput;
        }

        accel = moveSpeedComponent?.Acceleration ?? MovementSpeedModifierComponent.DefaultAcceleration;

        // This way friction never exceeds acceleration when you're trying to move.
        // If you want to slow down an entity with "friction" you shouldn't be using this system.
        if (wishDir != Vector2.Zero)
            friction = Math.Min(friction, accel);
        friction = Math.Max(friction, 0);
        var minimumFrictionSpeed = moveSpeedComponent?.MinimumFrictionSpeed ?? MovementSpeedModifierComponent.DefaultMinimumFrictionSpeed;
        Friction(minimumFrictionSpeed, frameTime, friction, ref velocity);
        
        Accelerate(ref velocity, in wishDir, accel, frameTime);

        SetWishDir((uid, mover), wishDir);

        /*
         * SNAKING!!! >-( 0 ================>
         * Snaking is a feature where you can move faster by strafing in a direction perpendicular to the
         * direction you intend to move while still holding the movement key for the direction you're trying to move.
         * Snaking only works if acceleration exceeds friction, and it's effectiveness scales as acceleration continues
         * to exceed friction.
         * Snaking works because friction is applied first in the direction of our current velocity, while acceleration
         * is applied after in our "Wish Direction" and is capped by the dot of our wish direction and current direction.
         * This means when you change direction, you're technically able to accelerate more than what the velocity cap
         * allows, but friction normally eats up the extra movement you gain.
         * By strafing as stated above you can increase your speed by about 1.4 (square root of 2).
         * This only works if friction is low enough so be sure that anytime you are letting a mob move in a low friction
         * environment you take into account the fact they can snake! Also be sure to lower acceleration as well to
         * prevent jerky movement!
         */
        PhysicsSystem.SetLinearVelocity(uid, velocity, body: physicsComponent);

        // Ensures that players do not spiiiiiiin
        PhysicsSystem.SetAngularVelocity(uid, 0, body: physicsComponent);
    }

    public Vector2 GetWishDir(Entity<InputMoverComponent?> mover)
    {
        if (!MoverQuery.Resolve(mover.Owner, ref mover.Comp, false))
            return Vector2.Zero;

        return mover.Comp.WishDir;
    }

    public void SetWishDir(Entity<InputMoverComponent> mover, Vector2 wishDir)
    {
        if (mover.Comp.WishDir.Equals(wishDir))
            return;

        mover.Comp.WishDir = wishDir;
        Dirty(mover);
    }

    public void LerpRotation(EntityUid uid, InputMoverComponent mover, float frameTime)
    {
        var angleDiff = Angle.ShortestDistance(mover.RelativeRotation, mover.TargetRelativeRotation);

        // if we've just traversed then lerp to our target rotation.
        if (!angleDiff.EqualsApprox(Angle.Zero, 0.001))
        {
            var adjustment = angleDiff * 5f * frameTime;
            var minAdjustment = 0.01 * frameTime;

            if (angleDiff < 0)
            {
                adjustment = Math.Min(adjustment, -minAdjustment);
                adjustment = Math.Clamp(adjustment, angleDiff, -angleDiff);
            }
            else
            {
                adjustment = Math.Max(adjustment, minAdjustment);
                adjustment = Math.Clamp(adjustment, -angleDiff, angleDiff);
            }

            mover.RelativeRotation = (mover.RelativeRotation + adjustment).FlipPositive();
            Dirty(uid, mover);
        }
        else if (!angleDiff.Equals(Angle.Zero))
        {
            mover.RelativeRotation = mover.TargetRelativeRotation.FlipPositive();
            Dirty(uid, mover);
        }
    }

    public void Friction(float minimumFrictionSpeed, float frameTime, float friction, ref Vector2 velocity)
    {
        var speed = velocity.Length();

        if (speed < minimumFrictionSpeed)
            return;

        // This equation is lifted from the Physics Island solver.
        // We re-use it here because Kinematic Controllers can't/shouldn't use the Physics Friction
        velocity *= Math.Clamp(1.0f - frameTime * friction, 0.0f, 1.0f);

    }

    public void Friction(float minimumFrictionSpeed, float frameTime, float friction, ref float velocity)
    {
        if (Math.Abs(velocity) < minimumFrictionSpeed)
            return;

        // This equation is lifted from the Physics Island solver.
        // We re-use it here because Kinematic Controllers can't/shouldn't use the Physics Friction
        velocity *= Math.Clamp(1.0f - frameTime * friction, 0.0f, 1.0f);

    }

    /// <summary>
    /// Adjusts the current velocity to the target velocity based on the specified acceleration.
    /// </summary>
    public static void Accelerate(ref Vector2 currentVelocity, in Vector2 velocity, float accel, float frameTime)
    {
        var wishDir = velocity != Vector2.Zero ? velocity.Normalized() : Vector2.Zero;
        var wishSpeed = velocity.Length();

        var currentSpeed = Vector2.Dot(currentVelocity, wishDir);
        var addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0f)
            return;

        var accelSpeed = accel * frameTime * wishSpeed;
        accelSpeed = MathF.Min(accelSpeed, addSpeed);

        currentVelocity += wishDir * accelSpeed;
    }

    private bool _relativeMovement = false;
    
    private Vector2 AssertValidWish(InputMoverComponent mover, float walkSpeed, float sprintSpeed)
    {
        var (walkDir, sprintDir) = GetVelocityInput(mover);

        var total = walkDir * walkSpeed + sprintDir * sprintSpeed;

        var parentRotation = GetParentGridAngle(mover);
        var wishDir = _relativeMovement ? parentRotation.RotateVec(total) : total;

        DebugTools.Assert(MathHelper.CloseToPercent(total.Length(), wishDir.Length()));

        return wishDir;
    }
}
