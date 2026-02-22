using System.Numerics;
using Robust.Shared.Console;
using Robust.Shared.Map;

namespace Content.Server.Maps;

public sealed class MappingCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override string Command => "mapping";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var mapSystem = _entMan.System<SharedMapSystem>();
        if (shell.Player == null
            || shell.Player.AttachedEntity == null)
            return;

        mapSystem.CreateMap(out var mapId, false);
        shell.ExecuteCommand($"tp 0 0 {mapId}");
    }
}
