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
            var mixture1 = atmosSystem.ResolveMixture("AirComposition", 1f);
            var entry1 = atmosSystem.GenerateGaxMixEntry(mixture1);
            Assert.Warn(entry1.ToString());

            var mixture2 = atmosSystem.ResolveMixture("AirPercentage", 1f);
            var entry2 = atmosSystem.GenerateGaxMixEntry(mixture2);
            Assert.Warn(entry2.ToString());
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
