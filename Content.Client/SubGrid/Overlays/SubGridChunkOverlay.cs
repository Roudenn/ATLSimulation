using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client.Subgrid.Overlays;

/// <summary>
/// Simply draws all boundaries of all subgrid chunks on the screen.
/// </summary>
public sealed class SubGridChunkOverlay : Overlay
{
    private readonly IEntityManager _entityManager;
    private readonly IEyeManager _eyeManager;
    private readonly SharedTransformSystem _xform;
    private readonly SharedSubGridSystem _subGrid;

    public SubGridChunkOverlay(IEntityManager entityManager, IEyeManager eyeManager, SharedTransformSystem xform, SharedSubGridSystem subGrid)
    {
        _entityManager = entityManager;
        _eyeManager = eyeManager;
        _xform = xform;
        _subGrid = subGrid;
    }

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = _eyeManager.GetWorldViewport();
        var boxVector = new Vector2i(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize);
        var query = _entityManager.EntityQueryEnumerator<SubGridChunkComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var subgrid, out var xform))
        {
            var worldPos = _xform.GetMapCoordinates(uid, xform);
            var worldAABB = Box2.CenteredAround(worldPos.Position, boxVector);
            if (!viewport.Intersects(worldAABB))
                continue;

            // Chunk coverage area
            args.WorldHandle.DrawRect(worldAABB, Color.White.WithAlpha(0.2f));
            // Chunk origin
            args.WorldHandle.DrawCircle(worldPos.Position, 0.25f, Color.Pink);
            // Chunk border
            args.WorldHandle.DrawRect(worldAABB.Enlarged(-0.05f), Color.DarkViolet, false);
        }
    }
}
