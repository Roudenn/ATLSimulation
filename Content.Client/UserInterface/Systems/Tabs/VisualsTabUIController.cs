using System.Diagnostics.CodeAnalysis;
using Content.Client.Overlays;
using Content.Client.UserInterface.Systems.Tabs.Widgets;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Tabs;

[UsedImplicitly]
public sealed class VisualsTabUIController : UIController
{
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [UISystemDependency] private readonly DebugOverlaySystem _debugOverlay = default!;

    private VisualsTab? VisualsTab => UIManager.GetActiveUIWidgetOrNull<GameTabContainer>()?.Visuals;

    public void LoadButton()
    {
        if (VisualsTab == null)
            return;

        VisualsTab.DebugSubGridChunksCheckBox.OnPressed += DebugSubGridChunksOnPressed;
        VisualsTab.DebugSubGridTilesCheckBox.OnPressed += DebugSubGridTilesOnPressed;
        VisualsTab.AtmosphereVisibilityCheckBox.OnPressed += AtmosCompositionOnPressed;
        VisualsTab.TemperatureVisibilityCheckBox.OnPressed += HeatMapOnPressed;
    }

    public void UnloadButton()
    {
        if (VisualsTab == null)
            return;

        VisualsTab.DebugSubGridChunksCheckBox.Pressed = false;
        VisualsTab.DebugSubGridTilesCheckBox.Pressed = false;
        VisualsTab.AtmosphereVisibilityCheckBox.Pressed = false;
        VisualsTab.TemperatureVisibilityCheckBox.Pressed = false;
        VisualsTab.DebugSubGridChunksCheckBox.OnPressed -= DebugSubGridChunksOnPressed;
        VisualsTab.DebugSubGridTilesCheckBox.OnPressed -= DebugSubGridTilesOnPressed;
        VisualsTab.AtmosphereVisibilityCheckBox.OnPressed -= AtmosCompositionOnPressed;
        VisualsTab.TemperatureVisibilityCheckBox.OnPressed -= HeatMapOnPressed;
    }

    private void DebugSubGridChunksOnPressed(BaseButton.ButtonEventArgs args)
    {
        if (!TryGetViewerEnt(out var ent))
            return;

        ent.Value.Comp.SubGridChunkOverlay = args.Button.Pressed;
        _debugOverlay.UpdateOverlays(ent.Value.AsNullable());
    }

    private void DebugSubGridTilesOnPressed(BaseButton.ButtonEventArgs args)
    {
        if (!TryGetViewerEnt(out var ent))
            return;

        ent.Value.Comp.SubGridTilesOverlay = args.Button.Pressed;
        _debugOverlay.UpdateOverlays(ent.Value.AsNullable());
    }

    private void HeatMapOnPressed(BaseButton.ButtonEventArgs args)
    {
        if (!TryGetViewerEnt(out var ent))
            return;

        ent.Value.Comp.HeatMapOverlay = args.Button.Pressed;
        _debugOverlay.UpdateOverlays(ent.Value.AsNullable());
    }

    private void AtmosCompositionOnPressed(BaseButton.ButtonEventArgs args)
    {
        if (!TryGetViewerEnt(out var ent))
            return;

        ent.Value.Comp.AtmosCompositionOverlay = args.Button.Pressed;
        _debugOverlay.UpdateOverlays(ent.Value.AsNullable());
    }

    private bool TryGetViewerEnt([NotNullWhen(true)] out Entity<DebugOverlayViewerComponent>? ent)
    {
        ent = null;
        var uid = _playerMan.LocalEntity;

        if (!_entMan.TryGetComponent<DebugOverlayViewerComponent>(uid, out var debugOverlay))
            return false;

        ent = (uid.Value, debugOverlay);
        return true;
    }
}
