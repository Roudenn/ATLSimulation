using Content.Shared.Atmospherics;

namespace Content.Shared.IoC;

public static class SharedContentIoC
{
    public static void Register(IDependencyCollection deps)
    {
        deps.Register<IGasPrototypeManager, GasPrototypeManager>();
    }
}