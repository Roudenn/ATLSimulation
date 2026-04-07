using Content.Client.Atmospherics;
using Content.Client.UserInterface.Systems.Tabs.Widgets;
using Content.Shared.Atmospherics.Factory;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Tabs;

[UsedImplicitly]
public sealed class SubgridEditorTabUIController : UIController
{
    [Dependency] private readonly GasMixtureFactory _gasFactory = default!;
    [UISystemDependency] private readonly AtmosphericsSystem _atmos = default!;

    private SubgridEditorTab? EditorTab => UIManager.GetActiveUIWidgetOrNull<GameTabContainer>()?.Editor;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnReload);
    }

    private void OnReload(PrototypesReloadedEventArgs args)
    {
        EditorTab?.UpdateGasOptions();
    }

    public void LoadButton()
    {
        if (EditorTab == null)
            return;

        EditorTab.OnModeSelected += OnModeSelected;
        EditorTab.OnValueChanged += OnValueChanged;
        EditorTab.OnGasSelected += OnGasSelected;
        EditorTab?.UpdateGasOptions();
    }

    public void UnloadButton()
    {
        if (EditorTab == null)
            return;

        EditorTab.OnModeSelected -= OnModeSelected;
        EditorTab.OnValueChanged -= OnValueChanged;
        EditorTab.OnGasSelected -= OnGasSelected;
    }

    private void OnModeSelected(int id)
    {
        var selected = (SubGridPlacementMode) id;
        EditorTab?.SetGasOptionVisible(selected is SubGridPlacementMode.Moles);
        _atmos.CurrentMode = selected;
    }

    private void OnGasSelected(byte gasId)
    {
        _atmos.SelectedGas = _gasFactory[gasId].ID;
    }

    private void OnValueChanged(float value)
    {
        _atmos.ChangeAmount = value;
    }
}
