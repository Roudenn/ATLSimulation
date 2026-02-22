using Content.Server.GameTicking;
using Robust.Shared.Console;

namespace Content.Server.Maps;

public sealed class SetMapCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override string Command => "setmap";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        string? map;
        switch (args.Length)
        {
            case 1:
                map = args[0];
                break;
            default:
                return;
        }

        _entMan.System<GameTicker>().CurrentSimulationMap = map;
    }
}
