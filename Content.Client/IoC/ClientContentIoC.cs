using Content.Client.Clickable;
using Content.Client.Fullscreen;
using Content.Client.Parallax.Managers;
using Content.Client.Screenshot;
using Content.Client.Viewport;
using Content.Shared.IoC;

namespace Content.Client.IoC;

internal static class ClientContentIoC
{
    public static void Register(IDependencyCollection collection)
    {
        SharedContentIoC.Register(collection);
        collection.Register<IParallaxManager, ParallaxManager>();
        collection.Register<GeneratedParallaxCache>();
        collection.Register<IScreenshotHook, ScreenshotHook>();
        collection.Register<FullscreenHook, FullscreenHook>();
        collection.Register<IClickMapManager, ClickMapManager>();
        collection.Register<ViewportManager, ViewportManager>();
    }
}
