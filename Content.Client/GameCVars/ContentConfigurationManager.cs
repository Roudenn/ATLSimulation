using System.Globalization;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Client.GameCVars;

public sealed class ContentConfigurationManager
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    private ISawmill _sawmill = Logger.GetSawmill("content_config");

    public void SetCVar(string name, object value)
    {
        _conHost.ExecuteCommand($"sudo cvar {name} {value}");
    }

    public void SetCVar<T>(CVarDef<T> def, T value) where T : notnull
    {
        _sawmill.Info($"Default: {value.ToString()}");
        if (value is IFormattable format)
        {
            var culture = format.ToString(null, CultureInfo.InvariantCulture);
            _sawmill.Info($"With Culture: {culture}");
            _conHost.ExecuteCommand($"sudo cvar {def.Name} {culture}");
        }
        else
        {
            _conHost.ExecuteCommand($"sudo cvar {def.Name} {value}");
            _sawmill.Info($"Fallback: {value.ToString()}");
        }
    }
}
