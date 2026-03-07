using Content.Server.GameTicking;
using Content.Server.SubGrid;
using Content.Shared.Atmospherics.GasMixtures;
using Robust.Shared.GameObjects;

namespace Content.Shared.IntegrationTests.Atmospherics;

[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture, TestOf(typeof(GasMixtureHelpers))]
public sealed class SubGridTest
{
    public async Task Test()
    {
        var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var system = server.EntMan.System<SubGridSystem>();
        var mapSystem = server.EntMan.System<SharedMapSystem>();
        var ticker = server.EntMan.System<GameTicker>();
        await server.WaitPost(() =>
        {

        });

        await pair.CleanReturnAsync();
    }
}
