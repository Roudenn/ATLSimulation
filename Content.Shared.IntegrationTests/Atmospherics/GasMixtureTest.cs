using Content.Shared.Atmospherics.GasMixtures;

namespace Content.Shared.IntegrationTests.Atmospherics;

[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture, TestOf(typeof(GasMixtureHelpers))]
public sealed class GasMixtureTest
{
    public async Task Test()
    {
        var pair = await PoolManager.GetServerClient();
        
    }
}