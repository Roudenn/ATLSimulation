#nullable enable
using Content.Client.IoC;
using Content.Client.Parallax.Managers;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.UnitTesting;

namespace Content.UnitTesting;

/// <summary>
/// This object wraps a pooled server+client pair.
/// </summary>
public sealed partial class TestPair : RobustIntegrationTest.TestPair
{
    public override async Task RevertModifiedCvars()
    {
        await base.RevertModifiedCvars();
        ClearModifiedCvars();
    }

    protected override RobustIntegrationTest.ClientIntegrationOptions ClientOptions()
    {
        var opts = base.ClientOptions();

        opts.LoadTestAssembly = false;
        opts.ContentStart = true;
        opts.FailureLogLevel = LogLevel.Warning;
        opts.Options = new()
        {
            LoadConfigAndUserData = false,
        };

        opts.BeforeStart += () =>
        {
            IoCManager.Resolve<IModLoader>().SetModuleBaseCallbacks(new ClientModuleTestingCallbacks
                {
                    ClientBeforeIoC = () => IoCManager.Register<IParallaxManager, DummyParallaxManager>(true)
                });
        };
        return opts;
    }

    protected override RobustIntegrationTest.ServerIntegrationOptions ServerOptions()
    {
        var opts = base.ServerOptions();

        opts.LoadTestAssembly = false;
        opts.ContentStart = true;
        opts.Options = new()
        {
            LoadConfigAndUserData = false,
        };

        /*opts.BeforeStart += () =>
        {
            // Server-only systems (i.e., systems that subscribe to events with server-only components)
            // There's probably a better way to do this.
            var entSysMan = IoCManager.Resolve<IEntitySystemManager>();
            entSysMan.LoadExtraSystemType<DeviceNetworkTestSystem>();
            entSysMan.LoadExtraSystemType<TestDestructibleListenerSystem>();
        };*/
        return opts;
    }
}
