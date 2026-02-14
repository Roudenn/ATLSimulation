using Content.Client.UserInterface.Controls;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controllers.Implementations;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.TilePlacement;

[UsedImplicitly]
public sealed class ContentTilePlacementUIController : UIController
{
    [Dependency] private readonly IInputManager _inputManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        _inputManager.SetInputCommand(ContentKeyFunctions.ToggleTileSpawningWindow,
            InputCmdHandler.FromDelegate(_ => ToggleWindow()));
    }

    private void ToggleWindow()
    {
        UIManager.GetUIController<TileSpawningUIController>().ToggleWindow();
    }

    private MenuButton? TilePlacementButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.TilePlacementButton;

    public void UnloadButton()
    {
        if (TilePlacementButton == null)
        {
            return;
        }

        TilePlacementButton.OnPressed -= TilePlacementButtonPressed;
    }

    public void LoadButton()
    {
        if (TilePlacementButton == null)
        {
            return;
        }

        TilePlacementButton.OnPressed += TilePlacementButtonPressed;
    }

    private void TilePlacementButtonPressed(BaseButton.ButtonEventArgs args)
    {
        ToggleWindow();
    }
}
