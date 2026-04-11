using System.Numerics;
using Content.Client.UserInterface.Systems;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.Atmospherics.Overlays;

/// <summary>
/// Shows the temperature of all heat containers on the map.
/// </summary>
public sealed class AtmosPressureOverlay : Overlay
{
    private readonly IEntityManager _entityManager;
    private readonly IEyeManager _eyeManager;
    private readonly GasMixtureFactory _gasManager;
    private readonly SharedTransformSystem _xform;
    private readonly SharedSubGridSystem _subGrid;

    public AtmosPressureOverlay(IEntityManager entityManager, IEyeManager eyeManager, GasMixtureFactory gasManager, SharedTransformSystem xform, SharedSubGridSystem subGrid)
    {
        _entityManager = entityManager;
        _eyeManager = eyeManager;
        _gasManager = gasManager;
        _xform = xform;
        _subGrid = subGrid;
    }

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = _eyeManager.GetWorldViewport();
        var boxVector = new Vector2i(SystemConstants.PvsChunkSize, SystemConstants.PvsChunkSize);
        var query = _entityManager.EntityQueryEnumerator<SubGridChunkComponent, TransformComponent>();
        var tileWorldSize = new Vector2(_subGrid.SubGridWorldSize);
        while (query.MoveNext(out var uid, out var subgrid, out var xform))
        {
            var worldPos = _xform.GetMapCoordinates(uid, xform);
            var worldAABB = Box2.CenteredAround(worldPos.Position, boxVector);
            if (!viewport.Intersects(worldAABB))
                continue;

            for (int i = 0; i < subgrid.ChunkData.AtmosphereMap.Length; i++)
            {
                var tile = subgrid.ChunkData.AtmosphereMap[i];
                if (!tile.Initialized)
                    continue;

                var pos = _subGrid.GetPositionFromIndex(subgrid.ChunkIndices, i);
                var worldTilePos = _xform.ToMapCoordinates(new EntityCoordinates(subgrid.ParentGrid, pos));
                var box = Box2.CenteredAround(worldTilePos.Position + tileWorldSize / 2, tileWorldSize);
                var pressure = _gasManager.GetPressure(ref tile.CachedMixture);
                args.WorldHandle.DrawRect(box,
                    ProgressColorHelpers.GradientCold(
                            pressure,
                            0f,
                            1000f)
                        .WithAlpha(0.5f));
            }
        }
    }
}
