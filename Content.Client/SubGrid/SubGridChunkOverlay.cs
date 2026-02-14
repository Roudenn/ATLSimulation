using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map.Components;

namespace Content.Client.Subgrid;

/// <summary>
/// Simply draws all boundaries of all subgrid chunks on the screen.
/// </summary>
public sealed class SubGridChunkOverlay : Overlay
{
    private readonly IEntityManager _entityManager;
    private readonly IEyeManager _eyeManager;
    private readonly SharedTransformSystem _xform;

    public SubGridChunkOverlay(IEntityManager entityManager, IEyeManager eyeManager, SharedTransformSystem xform)
    {
        _entityManager = entityManager;
        _eyeManager = eyeManager;
        _xform = xform;
    }

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = _eyeManager.GetWorldViewport();
        var boxVector = new Vector2i(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize);
        var query = _entityManager.EntityQueryEnumerator<SubGridChunkComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            var worldPos = _xform.GetMapCoordinates(uid, xform);
            var worldAABB = Box2.CenteredAround(worldPos.Position, boxVector);
            if (!viewport.Intersects(worldAABB))
                continue;

            args.WorldHandle.DrawRect(worldAABB, Color.White.WithAlpha(0.3f));
        }
    }

    public Entity<MapGridComponent> Grid { get; set; }
    public bool RequiresFlush { get; set; }
}
