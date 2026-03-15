namespace Content.Shared.IntegrationTests.Atmospherics;

[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture]
public sealed class GasMixtureTest
{
    public async Task Test()
    {
        var pair = await PoolManager.GetServerClient();

    }
}
