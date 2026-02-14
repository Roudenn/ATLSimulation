using Content.Client.UserInterface.Systems.Gameplay;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.UserInterface.Systems.Tabs;

[UsedImplicitly]
public sealed class TabsUIController : UIController
{
    [Dependency] private readonly VisualsTabUIController _visuals = default!;

    //private StatisticsTab? StatisticsTab => UIManager.GetActiveUIWidgetOrNull<GameTabContainer>()?.Statistics;
    //private SimulationOptionsTab? SimulationOptionsTab => UIManager.GetActiveUIWidgetOrNull<GameTabContainer>()?.SimulationOptions;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += LoadButtons;
        gameplayStateLoad.OnScreenUnload += UnloadButtons;
    }

    public void LoadButtons()
    {
        _visuals.LoadButton();
    }

    public void UnloadButtons()
    {
        _visuals.UnloadButton();
    }
}
