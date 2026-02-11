using Content.Shared.Atmospherics.GasMixtures;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Content.Shared.Tests;

/// <summary>
/// Tests basic methods for working with gas mixtures that don't require information from other systems.
/// </summary>
[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture, TestOf(typeof(GasMixtureHelpers))]
public sealed class GasMixtureHelpersTest
{
    private readonly GasMixture Mixture = new GasMixture();
}