using Content.Client.UserInterface.Systems.EscapeMenu;
using Content.Shared.Input;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controllers.Implementations;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.EntitySpawning;

public sealed class ContentEntitySpawningUIController : UIController
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        _inputManager.SetInputCommand(ContentKeyFunctions.ToggleEntitySpawningWindow,
            InputCmdHandler.FromDelegate(_ => ToggleWindow()));
    }

    private void ToggleWindow()
    {
        UIManager.GetUIController<EntitySpawningUIController>().ToggleWindow();
    }
}
