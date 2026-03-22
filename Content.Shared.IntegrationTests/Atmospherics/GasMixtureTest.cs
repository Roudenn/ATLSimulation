using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.Systems;
using Content.Shared.Constants;

namespace Content.Shared.IntegrationTests.Atmospherics;

[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture]
public sealed class GasMixtureTest
{
    [Test]
    public async Task TestPhysicalProperties()
    {
        var pair = await PoolManager.GetServerClient();
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
    public async Task TestDiffusion()
    {
        var pair = await PoolManager.GetServerClient();
        var factory = pair.Server.Resolve<GasMixtureFactory>();

        var atmosSystem = pair.Server.EntMan.System<SharedAtmosphericsSystem>();

        await pair.RunTicksSync(30);
        await pair.Server.WaitPost(() =>
        {
            Diffusion(factory, atmosSystem, "NitrogenPure", "OxygenPure");
            Diffusion(factory, atmosSystem, "OxygenPure0Degrees", "OxygenPure100Degrees");
            Diffusion(factory, atmosSystem, "OxygenPure0Degrees", "NitrogenPure100Degrees");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestDiffusionLoop()
    {
        var pair = await PoolManager.GetServerClient();
        var factory = pair.Server.Resolve<GasMixtureFactory>();

        var atmosSystem = pair.Server.EntMan.System<SharedAtmosphericsSystem>();

        await pair.RunTicksSync(30);
        await pair.Server.WaitPost(() =>
        {
            DiffusionLoop(factory, atmosSystem, "OxygenPure0Degrees", "NitrogenPure100Degrees", 100);
        });

        await pair.CleanReturnAsync();
    }

    private void Diffusion(GasMixtureFactory factory, SharedAtmosphericsSystem atmosSystem, string firstMixture, string secondMixture)
    {
        var m1 = atmosSystem.ResolveMixture(firstMixture, 2.5f);
        var m2 = atmosSystem.ResolveMixture(secondMixture, 2.5f);
        var diffusion1 = new float[factory.ArraySize];
        var diffusion2 = new float[factory.ArraySize];
        factory.DiffuseMixtures(ref m1, ref m2, ref diffusion1, ref diffusion2, 2.5f, 1f);
        string result = string.Empty;
        foreach (var d in diffusion1)
        {
            if (MathF.Abs(d) <= SystemConstants.GasMinMoles)
                continue;

            result += $"{d} \n";
        }
        foreach (var d in diffusion2)
        {
            if (MathF.Abs(d) <= SystemConstants.GasMinMoles)
                continue;

            result += $"{d} \n";
        }
        result += $"Temp 1: {m1.Temperature:0.00} \n";
        result += $"Temp 2: {m2.Temperature:0.00} \n";

        Assert.Warn(result);
    }

    private void DiffusionLoop(GasMixtureFactory factory, SharedAtmosphericsSystem atmosSystem, string firstMixture, string secondMixture, int iterations)
    {
        var m1 = atmosSystem.ResolveMixture(firstMixture, 2.5f);
        var m2 = atmosSystem.ResolveMixture(secondMixture, 2.5f);
        string result = string.Empty;
        for (int i = 0; i < iterations; i++)
        {
            var diffusion1 = new float[factory.ArraySize];
            var diffusion2 = new float[factory.ArraySize];
            factory.DiffuseMixtures(ref m1, ref m2, ref diffusion1, ref diffusion2, 2.5f, 1f);

            if (iterations % 10 != 0)
                continue;

            result += "Diffused moles: \n";
            foreach (var d in diffusion1)
            {
                if (MathF.Abs(d) <= SystemConstants.GasMinMoles)
                    continue;

                result += $"{d} \n";
            }
            foreach (var d in diffusion2)
            {
                if (MathF.Abs(d) <= SystemConstants.GasMinMoles)
                    continue;

                result += $"{d} \n";
            }
            result += "Mixture 1 Air composition: \n";
            foreach (var m in m1.Moles)
            {
                if (MathF.Abs(m) <= SystemConstants.GasMinMoles)
                    continue;

                result += $"{m:0.0000} \n";
            }
            result += "Mixture 2 Air composition: \n";
            foreach (var m in m2.Moles)
            {
                if (MathF.Abs(m) <= SystemConstants.GasMinMoles)
                    continue;

                result += $"{m:0.0000} \n";
            }
            result += $"Temp 1: {m1.Temperature:0.00} \n";
            result += $"Temp 2: {m2.Temperature:0.00} \n";
        }
        Assert.Warn(result);
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
