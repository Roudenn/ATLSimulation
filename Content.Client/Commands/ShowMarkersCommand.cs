using Content.Client.Markers;
using Robust.Shared.Console;

namespace Content.Client.Commands;

internal sealed class ShowMarkersCommand : LocalizedEntityCommands
{
    [Dependency] private readonly MarkerSystem _markerSystem = default!;

    public override string Command => "showmarkers";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _markerSystem.MarkersVisible ^= true;
    }
}
