using Content.Shared.Atmospherics;
using Content.Shared.Localizations;

namespace Content.Shared.IoC;

public static class SharedContentIoC
{
    public static void Register(IDependencyCollection deps)
    {
        deps.Register<GasPrototypeManager, GasPrototypeManager>();
        deps.Register<ContentLocalizationManager, ContentLocalizationManager>();
    }
}
