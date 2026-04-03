using System.Numerics;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Constants;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.Atmospherics.Overlays;

/// <summary>
/// Shows gas composition of the atmosphere.
/// </summary>
public sealed class AtmosCompositionOverlay : Overlay
{
    private readonly IEntityManager _entityManager;
    private readonly IEyeManager _eyeManager;
    private readonly GasMixtureFactory _gasManager;
    private readonly SharedTransformSystem _xform;
    private readonly SharedSubGridSystem _subGrid;

    public AtmosCompositionOverlay(IEntityManager entityManager, IEyeManager eyeManager, GasMixtureFactory gasManager, SharedTransformSystem xform, SharedSubGridSystem subGrid)
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
        var tileWorldSize = new Vector2(1f / _subGrid.SubGridTileSize);
        var buffer = _gasManager.SharedPool.Rent();
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
                var box = Box2.CenteredAround(worldTilePos.Position + tileWorldSize / 2f, tileWorldSize);

                for (int j = 0; j < _gasManager.ArraySize; j++)
                {
                    if (tile.ArchivedMixture.Moles[j] < SystemConstants.GasMinMoles)
                        continue;

                    var color = _gasManager[j].Color;
                    for (int k = 0; k < buffer.Length; k++)
                    {
                        buffer[k] = 0f;
                    }
                    _gasManager.GetMolesRatio(ref tile.ArchivedMixture, buffer);
                    var alpha = buffer[j] / 4f;
                    args.WorldHandle.DrawRect(box, color.WithAlpha(alpha));
                }
            }
        }
        _gasManager.SharedPool.Return(buffer, true);
    }
}
