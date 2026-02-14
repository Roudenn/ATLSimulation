using Content.Client.UserInterface.Systems.EntitySpawning;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Client.UserInterface.Systems.TilePlacement;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.UserInterface.Systems.MenuBar;

public sealed class GameTopMenuBarUIController : UIController
{
    [Dependency] private readonly EscapeUIController _escape = default!;
    [Dependency] private readonly ContentEntitySpawningUIController _entSpawning = default!;
    [Dependency] private readonly ContentTilePlacementUIController _tilePlacement = default!;

    private GameTopMenuBar? GameTopMenuBar => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>();

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += LoadButtons;
        gameplayStateLoad.OnScreenUnload += UnloadButtons;
    }

    public void UnloadButtons()
    {
        _escape.UnloadButton();
        _entSpawning.UnloadButton();
        _tilePlacement.UnloadButton();
    }

    public void LoadButtons()
    {
        _escape.LoadButton();
        _entSpawning.LoadButton();
        _tilePlacement.LoadButton();
    }
}
