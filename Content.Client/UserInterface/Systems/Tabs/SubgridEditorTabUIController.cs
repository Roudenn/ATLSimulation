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

        EditorTab.OnHeatValueChanged += OnHeatValueChanged;
        EditorTab.OnGasValueChanged += OnGasValueChanged;
        EditorTab.OnGasSelected += OnGasSelected;
        EditorTab?.UpdateGasOptions();
    }

    public void UnloadButton()
    {
        if (EditorTab == null)
            return;

        EditorTab.OnHeatValueChanged -= OnHeatValueChanged;
        EditorTab.OnGasValueChanged -= OnGasValueChanged;
        EditorTab.OnGasSelected -= OnGasSelected;
    }

    private void OnGasSelected(int gasId)
    {
        _atmos.SelectedGas = gasId == -1 ? null : _gasFactory[gasId].ID;
    }

    private void OnGasValueChanged(float value)
    {
        _atmos.GasChangeAmount = value;
    }

    private void OnHeatValueChanged(float value)
    {
        _atmos.HeatChangeAmount = value;
    }
}
