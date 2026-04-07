using System.Numerics;
using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Systems;
using Content.Shared.Input;
using Content.Shared.Subgrid;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Atmospherics;

public sealed class AtmosphericsSystem : SharedAtmosphericsSystem
{
    [Dependency] private readonly IInputManager _inputMan = default!;
    [Dependency] private readonly IEyeManager _eyeMan = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public SubGridPlacementMode CurrentMode = SubGridPlacementMode.None;

    public float ChangeAmount = 0f;

    public ProtoId<GasPrototype>? SelectedGas = null;

    public override void Initialize()
    {
        base.Initialize();
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.AdjustSubgridElement,
                InputCmdHandler.FromDelegate(_ => AdjustSubgridElement()))
            .Bind(ContentKeyFunctions.ReduceSubgridElement,
                InputCmdHandler.FromDelegate(_ => ReduceSubgridElement()))
            .Register<SharedAtmosphericsSystem>();
    }

    private void AdjustSubgridElement()
    {
        var mousePos = _eyeMan.PixelToMap(_inputMan.MouseScreenPosition);
        if (!_mapMan.TryFindGridAt(mousePos, out var gridUid, out var gridComp))
            return;

        var localPos = Vector2.Transform(mousePos.Position, _xform.GetInvWorldMatrix(gridUid));
        var tile = _map.LocalToTile(gridUid, gridComp, new EntityCoordinates(gridUid, localPos));

        switch (CurrentMode)
        {
            case SubGridPlacementMode.None:
                return;
            case SubGridPlacementMode.Moles:
                if (SelectedGas == null)
                    return;

                var atmosEv = new SubGridAddMolesMessage(GetNetEntity(gridUid), tile, SelectedGas.Value, ChangeAmount);
                RaiseNetworkEvent(atmosEv);
                break;
            case SubGridPlacementMode.Temperature:
                var heatEv = new SubGridAddHeatMessage(GetNetEntity(gridUid), tile, ChangeAmount);
                RaiseNetworkEvent(heatEv);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ReduceSubgridElement()
    {
        var mousePos = _eyeMan.PixelToMap(_inputMan.MouseScreenPosition);
        if (!_mapMan.TryFindGridAt(mousePos, out var gridUid, out var gridComp))
            return;

        var localPos = Vector2.Transform(mousePos.Position, _xform.GetInvWorldMatrix(gridUid));
        var tile = _map.LocalToTile(gridUid, gridComp, new EntityCoordinates(gridUid, localPos));

        switch (CurrentMode)
        {
            case SubGridPlacementMode.None:
                return;
            case SubGridPlacementMode.Moles:
                if (SelectedGas == null)
                    return;

                var atmosEv = new SubGridAddMolesMessage(GetNetEntity(gridUid), tile, SelectedGas.Value, -ChangeAmount);
                RaisePredictiveEvent(atmosEv);
                break;
            case SubGridPlacementMode.Temperature:
                var heatEv = new SubGridAddHeatMessage(GetNetEntity(gridUid), tile, -ChangeAmount);
                RaisePredictiveEvent(heatEv);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum SubGridPlacementMode
{
    None,
    Moles,
    Temperature,
}
