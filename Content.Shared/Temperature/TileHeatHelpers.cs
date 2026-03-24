using Content.Shared.Temperature.HeatContainers;
using JetBrains.Annotations;

namespace Content.Shared.Temperature;

public static class TileHeatHelpers
{
    [PublicAPI]
    public static float ConductHeatTiles(ref TileHeat tA, ref TileHeat tB, float deltaTime)
    {
        return ConductHeatArchived(
            ref tA.ArchivedContainer,
            ref tB.ArchivedContainer,
            ref tA.Container,
            ref tB.Container,
            deltaTime);
    }

    /// <summary>
    /// Conducts heat between two <see cref="IHeatContainer"/>s,
    /// given some constant thermal conductance g and a small time delta.
    /// </summary>
    /// <param name="cA">The first <see cref="IHeatContainer"/> to conduct heat to.</param>
    /// <param name="cB">The second <see cref="IHeatContainer"/> to conduct heat to.</param>
    /// <param name="cAUnarchived">Unarchived version of container cA to apply the conducted heat to.</param>
    /// <param name="cBUnarchived">Unarchived version of container cB to apply the conducted heat to.</param>
    /// <param name="deltaTime">
    /// The amount of time that the heat is allowed to conduct, in seconds.
    /// This value should be small such that deltaTime &lt;&lt; C / g where C is the heat capacity of the containers.
    /// If you need to simulate a larger time step split it into several smaller ones.
    /// </param>
    /// <returns>The amount of heat in joules that is exchanged between the bodies.</returns>
    /// <example>A positive value indicates heat transfer from a hot cB to a cold cA.</example>
    /// <remarks>
    /// This performs a single step using the Euler method for solving the Fourier heat equation
    /// \frac{dQ}{dt} = g \Delta T.
    /// If we need more precision in the future consider using a higher order integration scheme.
    /// If we need support for larger time steps in the future consider adding a method to split the time delta into several
    /// integration steps with adaptive step size.
    /// </remarks>
    [PublicAPI]
    public static float ConductHeatArchived(
        ref ConductiveHeatContainer cA,
        ref ConductiveHeatContainer cB,
        ref ConductiveHeatContainer cAUnarchived,
        ref ConductiveHeatContainer cBUnarchived,
        float deltaTime)
    {
        var dQ = cA.ConductHeatQuery(ref cB, deltaTime);
        HeatContainerHelpers.AddHeat(ref cAUnarchived, dQ);
        HeatContainerHelpers.AddHeat(ref cBUnarchived, -dQ);
        return dQ;
    }
}
