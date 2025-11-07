using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.CloseWindow;

public sealed class CloseAllWindowsUIController : UIController
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    public override void Initialize()
    {
        _inputManager.SetInputCommand(EngineKeyFunctions.WindowCloseAll,
            InputCmdHandler.FromDelegate(_ => CloseAllWindows()));
    }

    private void CloseAllWindows()
    {
        foreach (var childControl in new List<Control>(_uiManager.WindowRoot.Children)) // Copy children list as it will be modified on Close()
        {
            if (childControl is BaseWindow)
            {
                ((BaseWindow) childControl).Close();
            }
        }
    }
}

