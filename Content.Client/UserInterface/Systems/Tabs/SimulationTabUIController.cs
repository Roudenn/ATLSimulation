using Content.Client.UserInterface.Systems.Tabs.Widgets;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Tabs;

[UsedImplicitly]
public sealed class SimulationTabUIController : UIController
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    private SimulationOptionsTab? SimulationTab => UIManager.GetActiveUIWidgetOrNull<GameTabContainer>()?.SimulationOptions;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    private void OnPrototypeReload(PrototypesReloadedEventArgs obj)
    {
        if (!obj.WasModified<SimulationMapPrototype>())
            return;

        SimulationTab?.UpdateMapsList();
    }

    public void LoadButton()
    {
        if (SimulationTab == null)
            return;

        SimulationTab.UpdateMapsList();
        SimulationTab.OnMapSelected += OnMapSelected;
        SimulationTab.OnRestart += OnRestart;
    }

    public void UnloadButton()
    {
        if (SimulationTab == null)
            return;

        SimulationTab.OnMapSelected -= OnMapSelected;
        SimulationTab.OnRestart -= OnRestart;
    }

    private void OnMapSelected()
    {
        if (SimulationTab?.SelectedMap != null)
            _conHost.ExecuteCommand($"setmap {SimulationTab?.SelectedMap}");
    }

    private void OnRestart()
    {
        _conHost.ExecuteCommand("restart");
    }
}
