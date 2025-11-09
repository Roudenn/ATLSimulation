using Content.Client.Fullscreen;
using Content.Client.Input;
using Content.Client.IoC;
using Content.Client.MainMenu;
using Content.Client.Parallax.Managers;
using Content.Client.Screenshot;
using Content.Client.Viewport;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;

namespace Content.Client;

[UsedImplicitly]
public sealed class EntryPoint : GameClient
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IParallaxManager _parallaxManager = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IScreenshotHook _screenshotHook = default!;
    [Dependency] private readonly FullscreenHook _fullscreenHook = default!;
    [Dependency] private readonly ViewportManager _viewportManager = default!;
    
    public override void PreInit()
    {
        ClientContentIoC.Register(Dependencies);
    }

    public override void Init()
    {
        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);

        _componentFactory.DoAutoRegistrations();
        _componentFactory.IgnoreMissingComponents();

        foreach (var ignoreName in IgnoredComponents.List)
        {
            _componentFactory.RegisterIgnore(ignoreName);
        }

        foreach (var ignoreName in IgnoredPrototypes.List)
        {
            _prototypeManager.RegisterIgnore(ignoreName);
        }

        _componentFactory.GenerateNetIds();
        
        _screenshotHook.Initialize();
        _fullscreenHook.Initialize();
        _viewportManager.Initialize();
        
        //AUTOSCALING default Setup!
        _configManager.SetCVar("interface.resolutionAutoScaleUpperCutoffX", 1080);
        _configManager.SetCVar("interface.resolutionAutoScaleUpperCutoffY", 720);
        _configManager.SetCVar("interface.resolutionAutoScaleLowerCutoffX", 520);
        _configManager.SetCVar("interface.resolutionAutoScaleLowerCutoffY", 240);
        _configManager.SetCVar("interface.resolutionAutoScaleMinimum", 0.5f);
    }

    public override void PostInit()
    {
        base.PostInit();
        
        // Setup key contexts
        ContentContexts.SetupContexts(_inputManager.Contexts);
        
        _userInterfaceManager.SetDefaultTheme("SS14DefaultTheme");
        _userInterfaceManager.SetActiveTheme(_configManager.GetCVar(CVars.InterfaceTheme));
        _parallaxManager.LoadDefaultParallax();
        
        _lightManager.Enabled = false; // We don't need any lighting in a demo
        
        // Disable engine-default viewport since we use our own custom viewport control.
        _userInterfaceManager.MainViewport.Visible = false;
        
        _stateManager.RequestStateChange<MainScreen>();
    }
}
