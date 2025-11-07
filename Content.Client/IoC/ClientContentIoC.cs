using Content.Client.Parallax.Managers;
using Content.Shared.IoC;

namespace Content.Client.IoC;

internal static class ClientContentIoC
{
    public static void Register(IDependencyCollection collection)
    {
        SharedContentIoC.Register(collection);
        collection.Register<IParallaxManager, ParallaxManager>();
        collection.Register<GeneratedParallaxCache>();
    }
}
