using Content.Shared.IoC;

namespace Content.Server.IoC;

internal static class ServerContentIoC
{
    public static void Register(IDependencyCollection deps)
    {
        SharedContentIoC.Register(deps);
    }
}
