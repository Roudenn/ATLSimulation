using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controllers.Implementations;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.EntitySpawning;

[UsedImplicitly]
public sealed class ContentEntitySpawningUIController : UIController
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        _inputManager.SetInputCommand(ContentKeyFunctions.ToggleEntitySpawningWindow,
            InputCmdHandler.FromDelegate(_ => ToggleEntityWindow()));
        
        _inputManager.SetInputCommand(ContentKeyFunctions.ToggleTileSpawningWindow,
            InputCmdHandler.FromDelegate(_ => ToggleTileWindow()));
    }

    private void ToggleEntityWindow()
    {
        UIManager.GetUIController<EntitySpawningUIController>().ToggleWindow();
    }
    
    private void ToggleTileWindow()
    {
        UIManager.GetUIController<TileSpawningUIController>().ToggleWindow();
    }
}
