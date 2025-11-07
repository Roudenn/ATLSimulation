using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Screens;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.Viewport;
using Content.Shared.GameCVars;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client.Gameplay
{
    [Virtual]
    public class GameplayState : GameplayStateBase, IMainViewportState
    {
        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly IOverlayManager _overlayManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IConfigurationManager _configurationManager = default!;

        private FpsCounter _fpsCounter = default!;

        public MainViewport Viewport => _uiManager.ActiveScreen!.GetWidget<MainViewport>()!;

        private readonly GameplayStateLoadController _loadController;

        public GameplayState()
        {
            IoCManager.InjectDependencies(this);

            _loadController = _uiManager.GetUIController<GameplayStateLoadController>();
        }

        protected override void Startup()
        {
            base.Startup();

            LoadMainScreen();

            // FPS counter.
            // yeah this can just stay here, whatever
            _fpsCounter = new FpsCounter(_gameTiming);
            UserInterfaceManager.PopupRoot.AddChild(_fpsCounter);
            _fpsCounter.Visible = _configurationManager.GetCVar(GameConfigVars.HudFpsCounterVisible);
            _configurationManager.OnValueChanged(GameConfigVars.HudFpsCounterVisible, (show) => { _fpsCounter.Visible = show; });
        }

        protected override void Shutdown()
        {
            base.Shutdown();
            // Clear viewport to some fallback, whatever.
            _eyeManager.MainViewport = UserInterfaceManager.MainViewport;
            _fpsCounter.Dispose();
            _uiManager.ClearWindows();
            UnloadMainScreen();
        }

        private void UnloadMainScreen()
        {
            _loadController.UnloadScreen();
            _uiManager.UnloadScreen();
        }

        private void LoadMainScreen()
        {
            _uiManager.LoadScreen<DefaultGameScreen>();
            _loadController.LoadScreen();
        }

        protected override void OnKeyBindStateChanged(ViewportBoundKeyEventArgs args)
        {
            if (args.Viewport == null)
                base.OnKeyBindStateChanged(new ViewportBoundKeyEventArgs(args.KeyEventArgs, Viewport.Viewport));
            else
                base.OnKeyBindStateChanged(args);
        }
    }
}
