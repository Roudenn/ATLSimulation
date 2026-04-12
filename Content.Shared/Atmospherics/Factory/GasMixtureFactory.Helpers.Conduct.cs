using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Temperature;
using Content.Shared.Temperature.HeatContainers;
using Content.Shared.Utils;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    public void ConductTiles(ref TileAtmos m, ref TileHeat c, float cLength, float deltaTime, float k, IRobustArrayPool<float> pool)
    {
        var dQ = ConductHeatQuery(ref c.Container, ref m.Mixture, cLength, cLength, deltaTime);
        HeatContainerHelpers.AddHeat(ref c.CachedContainer, dQ);
        AddHeat(ref m.CachedMixture, -dQ);
    }

    /// <inheritdoc cref="ConductHeatQuery{T1,T2}"/>
    [PublicAPI]
    public float ConductHeat<T1, T2>(ref T1 cA, ref T2 cB, float cLength, float heatG, float deltaTime)
        where T1 : IHeatContainer
        where T2 : IGasMixture
    {
        var dQ = ConductHeatQuery(ref cA, ref cB, cLength, cLength, deltaTime);
        HeatContainerHelpers.AddHeat(ref cA, dQ);
        AddHeat(ref cB, -dQ);
        return dQ;
    }

    /// <summary>
    /// Calculates the amount of heat that would be conducted between <see cref="IHeatContainer"/> and a <see cref="IGasMixture"/>,
    /// given some time delta. Does not modify the containers.
    /// </summary>
    /// <param name="c">The <see cref="IHeatContainer"/> to conduct heat to.</param>
    /// <param name="m">The <see cref="IGasMixture"/> to conduct heat to.</param>
    /// <param name="cLength">Characteristic length of both containers.</param>
    /// <param name="heatG">The thermal conductance of the heat container in watt per kelvin.</param>
    /// <param name="deltaTime">
    /// The amount of time that the heat is allowed to conduct, in seconds.
    /// This value should be small such that deltaTime &lt;&lt; C / g where C is the heat capacity of the container.
    /// If you need to simulate a larger time step split it into several smaller ones.
    /// </param>
    /// <returns>The amount of heat in joules that would be exchanged between the bodies.</returns>
    /// <example>A positive value indicates heat transfer from a hot c2 to a cold c1.</example>
    /// <remarks>
    /// This performs a single step using the Euler method for solving the Fourier heat equation
    /// \frac{dQ}{dt} = g \Delta T.
    /// </remarks>
    [PublicAPI]
    public float ConductHeatQuery<T1, T2>(ref T1 c, ref T2 m, float cLength, float heatG, float deltaTime)
        where T1 : IHeatContainer
        where T2 : IGasMixture
    {
        // The harmonic mean is used because conductance adds up inversely proportional.
        var mixtureG = GetThermalConductivity(ref m) * cLength;
        var g = 2f * heatG * mixtureG / (heatG + mixtureG);
        var dQ = g * (m.Temperature - c.Temperature) * deltaTime;
        var dQMax = Math.Min(Math.Abs(HeatContainerHelpers.ConductHeatToTempQuery(ref c, m.Temperature)),
            Math.Abs(ConductHeatToTempQuery(ref m, c.Temperature)));

        // Clamp the transferred heat amount in case we are overshooting the equilibrium temperature because our time step was too large.
        return Math.Clamp(dQ, -dQMax, dQMax);
    }

    /// <inheritdoc cref="ConductHeatQuery{T1}"/>
    [PublicAPI]
    public float ConductHeat<T>(ref ConductiveHeatContainer c, ref T m, float cLength, float deltaTime) where T : IGasMixture
    {
        var dQ = ConductHeatQuery(ref c, ref m, cLength, cLength, deltaTime);
        HeatContainerHelpers.AddHeat(ref c, dQ);
        AddHeat(ref m, -dQ);
        return dQ;
    }

    /// <summary>
    /// Calculates the amount of heat that would be conducted between two <see cref="IHeatContainer"/>s,
    /// given some time delta. Does not modify the containers.
    /// </summary>
    /// <param name="c">The <see cref="ConductiveHeatContainer"/> to conduct heat to.</param>
    /// <param name="m">The <see cref="IGasMixture"/> to conduct heat to.</param>
    /// <param name="cLength">Characteristic length of both containers.</param>
    /// <param name="deltaTime">
    /// The amount of time that the heat is allowed to conduct, in seconds.
    /// This value should be small such that deltaTime &lt;&lt; C / g where C is the heat capacity of the container.
    /// If you need to simulate a larger time step split it into several smaller ones.
    /// </param>
    /// <returns>The amount of heat in joules that would be exchanged between the bodies.</returns>
    /// <example>A positive value indicates heat transfer from a hot c2 to a cold c1.</example>
    /// <remarks>
    /// This performs a single step using the Euler method for solving the Fourier heat equation
    /// \frac{dQ}{dt} = g \Delta T.
    /// </remarks>
    [PublicAPI]
    public float ConductHeatQuery<T>(ref ConductiveHeatContainer c, ref T m, float cLength, float deltaTime) where T : IGasMixture
    {
        // The harmonic mean is used because conductance adds up inversely proportional.
        var mixtureG = GetThermalConductivity(ref m) * cLength;
        var g = 2f * c.ThermalConductance * mixtureG / (c.ThermalConductance + mixtureG);
        var dQ = g * (m.Temperature - c.Temperature) * deltaTime;
        var dQMax = Math.Min(Math.Abs(HeatContainerHelpers.ConductHeatToTempQuery(ref c, m.Temperature)),
            Math.Abs(ConductHeatToTempQuery(ref m, c.Temperature)));

        // Clamp the transferred heat amount in case we are overshooting the equilibrium temperature because our time step was too large.
        return Math.Clamp(dQ, -dQMax, dQMax);
    }

    /// <summary>
    /// Calculates the amount of heat that would be conducted between two <see cref="IHeatContainer"/>s,
    /// given some time delta. Does not modify the containers.
    /// </summary>
    /// <param name="c">The <see cref="IHeatContainer"/> to conduct heat to.</param>
    /// <param name="m">The <see cref="IGasMixture"/> to conduct heat to.</param>
    /// <param name="cLength">Characteristic length of a gas mixture</param>
    /// <param name="surfaceArea">Surface area of the gas flow and </param>
    /// <param name="speed">Speed of the gas that hits the solid surface.</param>
    /// <param name="deltaTime">
    /// The amount of time that the heat is allowed to conduct, in seconds.
    /// This value should be small such that deltaTime &lt;&lt; C / g where C is the heat capacity of the container.
    /// If you need to simulate a larger time step split it into several smaller ones.
    /// </param>
    /// <returns>The amount of heat in joules that would be exchanged between the bodies.</returns>
    /// <example>A positive value indicates heat transfer from a hot c2 to a cold c1.</example>
    /// <remarks>
    /// The calculations are done according to Newton's law of cooling.
    /// </remarks>
    [PublicAPI]
    public float ConductHeatVelocityQuery(ref ConductiveHeatContainer c, ref GasMixture m, float cLength, float surfaceArea, float speed, float deltaTime)
    {
        // The harmonic mean is used because conductance adds up inversely proportional.
        var mixtureG = GetThermalConductivity(ref m) * cLength;
        var g = 2f * c.ThermalConductance * mixtureG / (c.ThermalConductance + mixtureG);

        // Calculate Reynold's number: Re = √(ρvL / μ)
        // Multiplying on 1/1000 because viscosity is measured in µPa·s, which is 10e-6, or 10e-3 after the square root.
        var viscosity = GetViscosity(ref m);
        var reynold = MathF.Sqrt(GetDensity(ref m) * speed * cLength / (viscosity * 0.001f));
        var prantl = GetPrandtlNumber(ref m, GetThermalConductivity(ref m), viscosity);

        // Calculate the heat transfer coefficient using an approximation: h ≈ 0.664 * Re * ∛Pr * g.
        // TODO I didn't fact-check that enough, maybe it's a wrong formula.
        // Source: https://www.researchgate.net/publication/352146707_Review_of_Convective_Heat_Transfer_Modelling_in_CFD_Simulations_of_Fire-Driven_Flows
        var h = 0.664f * reynold * MathF.Cbrt(prantl) * g;

        var dQ = h * surfaceArea * (m.Temperature - c.Temperature) * deltaTime;
        var dQMax = Math.Min(Math.Abs(HeatContainerHelpers.ConductHeatToTempQuery(ref c, m.Temperature)),
            Math.Abs(ConductHeatToTempQuery(ref m, c.Temperature)));

        // Clamp the transferred heat amount in case we are overshooting the equilibrium temperature because our time step was too large.
        return Math.Clamp(dQ, -dQMax, dQMax);
    }

    [PublicAPI]
    public float ConductHeatToTempQuery<T>(ref T m, float targetTemp) where T : IGasMixture
    {
        return (targetTemp - m.Temperature) * GetHeatCapacity(ref m);
    }
}
