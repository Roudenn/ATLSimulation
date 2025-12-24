using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Shared.GameCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.Viewport;

public sealed class ViewportUIController : UIController
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    public static readonly Vector2i ViewportSize = (EyeManager.PixelsPerMeter * 21, EyeManager.PixelsPerMeter * 15);
    public const int ViewportHeight = 15;
    private MainViewport? Viewport => UIManager.ActiveScreen?.GetWidget<MainViewport>();

    public override void Initialize()
    {
        _configurationManager.OnValueChanged(GameConfigVars.ViewportMinimumWidth, _ => UpdateViewportRatio());
        _configurationManager.OnValueChanged(GameConfigVars.ViewportMaximumWidth, _ => UpdateViewportRatio());
        _configurationManager.OnValueChanged(GameConfigVars.ViewportWidth, _ => UpdateViewportRatio());
        _configurationManager.OnValueChanged(GameConfigVars.ViewportVerticalFit, _ => UpdateViewportRatio());

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        ReloadViewport();
    }

    private void UpdateViewportRatio()
    {
        if (Viewport == null)
        {
            return;
        }

        var min = _configurationManager.GetCVar(GameConfigVars.ViewportMinimumWidth);
        var max = _configurationManager.GetCVar(GameConfigVars.ViewportMaximumWidth);
        var width = _configurationManager.GetCVar(GameConfigVars.ViewportWidth);
        var verticalfit = _configurationManager.GetCVar(GameConfigVars.ViewportVerticalFit) && _configurationManager.GetCVar(GameConfigVars.ViewportStretch);

        if (verticalfit)
        {
            width = max;
        }
        else if (width < min || width > max)
        {
            width = GameConfigVars.ViewportWidth.DefaultValue;
        }

        Viewport.Viewport.ViewportSize = (EyeManager.PixelsPerMeter * width, EyeManager.PixelsPerMeter * ViewportHeight);
        Viewport.UpdateCfg();
    }

    public void ReloadViewport()
    {
        if (Viewport == null)
        {
            return;
        }

        UpdateViewportRatio();
        Viewport.Viewport.HorizontalExpand = true;
        Viewport.Viewport.VerticalExpand = true;
        _eyeManager.MainViewport = Viewport.Viewport;
    }

    public override void FrameUpdate(FrameEventArgs e)
    {
        if (Viewport == null)
        {
            return;
        }

        base.FrameUpdate(e);

        Viewport.Viewport.Eye = _eyeManager.CurrentEye;

        // verify that the current eye is not "null". Fuck IEyeManager.

        var ent = _playerMan.LocalEntity;
        if (_eyeManager.CurrentEye.Position != default || ent == null)
            return;

        _entMan.TryGetComponent(ent, out EyeComponent? eye);

        if (eye?.Eye == _eyeManager.CurrentEye
            && _entMan.GetComponent<TransformComponent>(ent.Value).MapID == MapId.Nullspace)
        {
            // nothing to worry about, the player is just in null space... actually that is probably a problem?
            return;
        }

        // Currently, this shouldn't happen. This likely happened because the main eye was set to null. When this
        // does happen it can create hard to troubleshoot bugs, so lets print some helpful warnings:
        Log.Warning($"Main viewport's eye is in nullspace (main eye is null?). Attached entity: {_entMan.ToPrettyString(ent.Value)}. Entity has eye comp: {eye != null}");
    }
}
