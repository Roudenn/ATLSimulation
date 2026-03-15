using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Localizations;

namespace Content.Shared.IoC;

public static class SharedContentIoC
{
    public static void Register(IDependencyCollection deps)
    {
        deps.Register<GasMixtureFactory, GasMixtureFactory>();
        deps.Register<ContentLocalizationManager, ContentLocalizationManager>();
    }
}
