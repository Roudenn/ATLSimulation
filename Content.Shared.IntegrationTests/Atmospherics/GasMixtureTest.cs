using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.Systems;

namespace Content.Shared.IntegrationTests.Atmospherics;

[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture]
public sealed class GasMixtureTest
{
    [Test]
    public async Task TestPhysicalProperties()
    {
        var pair = await PoolManager.GetServerClient();
        var protoMan = pair.Server.ProtoMan;
        var atmosSystem = pair.Server.EntMan.System<SharedAtmosphericsSystem>();

        await pair.RunTicksSync(30);
        await pair.Server.WaitPost(() =>
        {
            var mixture = atmosSystem.ResolveMixture("Air", 1f);
            var entry = atmosSystem.GenerateGaxMixEntry(mixture);
            Assert.Warn(entry.ToString());
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestServerClientSync()
    {
        var pair = await PoolManager.GetServerClient();
        var sGasFactory = pair.Server.Resolve<GasMixtureFactory>();
        var cGasFactory = pair.Client.Resolve<GasMixtureFactory>();


        await pair.CleanReturnAsync();
    }
}
