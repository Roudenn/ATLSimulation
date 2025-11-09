using Robust.Server.Console;
using Robust.Shared.Player;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;

namespace Content.Server.Commands;

public sealed class ContentConGroupController : IContentConGroupController, IPostInjectInit, IConGroupControllerImplementation
{
    [Dependency] private readonly IConGroupController _conGroup = default!;
    
    public void PostInject()
    {
        _conGroup.Implementation = this;
    }

    public bool CanCommand(ICommonSession session, string cmdName)
    {
        return true;
    }

    public bool CanAdminMenu(ICommonSession session)
    {
        return true;
    }

    public bool CheckInvokable(CommandSpec command, ICommonSession? user, out IConError? error)
    {
        error = null;
        return true;
    }

    public bool CanScript(ICommonSession session)
    {
        return true;
    }

    public bool CanAdminReloadPrototypes(ICommonSession session)
    {
        return true;
    }

    public bool CanAdminPlace(ICommonSession session)
    {
        return true;
    }
}
