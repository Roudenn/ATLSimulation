using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Client.GameCVars;

public sealed class ContentConfigurationManager
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    public void SetCVar(string name, object value)
    {
        _conHost.ExecuteCommand($"sudo cvar {name} {value}");
    }

    public void SetCVar<T>(CVarDef<T> def, T value) where T : notnull
    {
        _conHost.ExecuteCommand($"sudo cvar {def.Name} {value.ToString()}");
    }
}
