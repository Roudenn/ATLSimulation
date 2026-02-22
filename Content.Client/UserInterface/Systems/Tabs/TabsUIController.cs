using Content.Client.UserInterface.Systems.Gameplay;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.UserInterface.Systems.Tabs;

[UsedImplicitly]
public sealed class TabsUIController : UIController
{
    [Dependency] private readonly StatisticsTabUIController _statistics = default!;
    [Dependency] private readonly VisualsTabUIController _visuals = default!;
    [Dependency] private readonly SimulationTabUIController _simulation = default!;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += LoadButtons;
        gameplayStateLoad.OnScreenUnload += UnloadButtons;
    }

    public void LoadButtons()
    {
        _statistics.LoadButton();
        _visuals.LoadButton();
        _simulation.LoadButton();
    }

    public void UnloadButtons()
    {
        _statistics.UnloadButton();
        _visuals.UnloadButton();
        _simulation.UnloadButton();
    }
}
