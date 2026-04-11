using System.Numerics;
using Content.Shared.Input;
using Content.Shared.Subgrid;
using Content.Shared.Subgrid.Systems;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;

namespace Content.Client.Subgrid;

public sealed class SubGridSystem : SharedSubGridSystem
{
    [Dependency] private readonly IInputManager _inputMan = default!;
    [Dependency] private readonly IEyeManager _eyeMan = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;

    public override void Initialize()
    {
        base.Initialize();
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.InspectSubgridElement,
                InputCmdHandler.FromDelegate(_ => InspectSubgridTile()))
            .Register<SharedSubGridSystem>();
    }

    private void InspectSubgridTile()
    {
        var mousePos = _eyeMan.PixelToMap(_inputMan.MouseScreenPosition);
        if (!_mapMan.TryFindGridAt(mousePos, out var gridUid, out var grid)
            || !SubGridQuery.TryComp(gridUid, out var subGrid))
            return;

        var localPos = Vector2.Transform(mousePos.Position, Xform.GetInvWorldMatrix(gridUid));
        var index = GetTileAtPosition(localPos);

        if (!TryGetChunk((gridUid, subGrid), localPos, out var chunk))
            return;

        var atmosTile = chunk.Value.Comp.ChunkData.AtmosphereMap[index];
        if (atmosTile.Initialized)
        {
            var atmosEv = new InspectSubGridAtmosphereTile(atmosTile.CachedMixture, gridUid, GetChunkIndices(localPos), index);
            RaiseLocalEvent(ref atmosEv);
            return;
        }

        var heatTile = chunk.Value.Comp.ChunkData.TemperatureMap[index];
        if (heatTile.Initialized)
        {
            var atmosEv = new InspectSubGridHeatTile(heatTile.CachedContainer, gridUid, GetChunkIndices(localPos), index);
            RaiseLocalEvent(ref atmosEv);
            return;
        }
    }
}

