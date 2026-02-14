using Content.Server.GameTicking;
using Robust.Shared.Console;

namespace Content.Server.Commands.GameTicking;

public sealed class RestartCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _ent = default!;

    public override string Command => "restart";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _ent.System<GameTicker>().RestartSimulationMap();
    }
}
