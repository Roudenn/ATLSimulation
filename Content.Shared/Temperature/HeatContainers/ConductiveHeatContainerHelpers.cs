using JetBrains.Annotations;

namespace Content.Shared.Temperature.HeatContainers;

// Migrated to a separate class in order to not interfere with upstream code.
public static class ConductiveHeatContainerHelpers
{
    #region Conduct

    /// <summary>
    /// Conducts heat between a <see cref="ConductiveHeatContainer"/> and some other body with a different temperature,
    /// given a small time delta, using conductance coefficient g from the conductive container.
    /// </summary>
    /// <param name="c">The <see cref="ConductiveHeatContainer"/> to conduct heat to.</param>
    /// <param name="temp">The temperature of the second object that we are conducting heat with, in kelvin.</param>
    /// <param name="deltaTime">
    /// The amount of time that the heat is allowed to conduct, in seconds.
    /// This value should be small such that deltaTime &lt;&lt; C / g where C is the heat capacity of the container.
    /// If you need to simulate a larger time step split it into several smaller ones.
    /// </param>
    /// <returns>The amount of heat in joules that was added to the heat container.</returns>
    /// <example>A positive value indicates heat transfer from a hot body to a cold heat container c.</example>
    /// <remarks>
    /// This performs a single step using the Euler method for solving the Fourier heat equation
    /// \frac{dQ}{dt} = g \Delta T.
    /// If we need more precision in the future consider using a higher order integration scheme.
    /// If we need support for larger time steps in the future consider adding a method to split the time delta into several
    /// integration steps with adaptive step size.
    /// </remarks>
    [PublicAPI]
    public static float ConductHeat(this ref ConductiveHeatContainer c, float temp, float deltaTime)
    {
        var dQ = ConductHeatQuery(ref c, temp, deltaTime);
        HeatContainerHelpers.AddHeat(ref c, dQ);
        return dQ;
    }

    /// <summary>
    /// Conducts heat between two <see cref="IHeatContainer"/>s,
    /// given some constant thermal conductance g and a small time delta.
    /// </summary>
    /// <param name="cA">The first <see cref="IHeatContainer"/> to conduct heat to.</param>
    /// <param name="cB">The second <see cref="IHeatContainer"/> to conduct heat to.</param>
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
    public static float ConductHeat(this ref ConductiveHeatContainer cA, ref ConductiveHeatContainer cB, float deltaTime)
    {
        var dQ = ConductHeatQuery(ref cA, ref cB, deltaTime);
        HeatContainerHelpers.AddHeat(ref cA, dQ);
        HeatContainerHelpers.AddHeat(ref cB, -dQ);
        return dQ;
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
        var dQ = ConductHeatQuery(ref cA, ref cB, deltaTime);
        HeatContainerHelpers.AddHeat(ref cAUnarchived, dQ);
        HeatContainerHelpers.AddHeat(ref cBUnarchived, -dQ);
        return dQ;
    }

    /// <summary>
    /// Calculates the amount of heat that would be conducted between a <see cref="IHeatContainer"/> and some body with a different temperature,
    /// given some constant thermal conductance g and a small time delta.
    /// </summary>
    /// <param name="c">The <see cref="IHeatContainer"/> to conduct heat to.</param>
    /// <param name="temp">The temperature of the second object that we are conducting heat with, in kelvin.</param>
    /// <param name="deltaTime">
    /// The amount of time that the heat is allowed to conduct, in seconds.
    /// This value should be small such that deltaTime &lt;&lt; C / g where C is the heat capacity of the container.
    /// If you need to simulate a larger time step split it into several smaller ones.
    /// </param>
    /// <returns>The amount of heat in joules that would be exchanged between the bodies.</returns>
    /// <example>A positive value indicates heat transfer from a hot body to a cold heat container c.</example>
    /// <remarks>
    /// This performs a single step using the Euler method for solving the Fourier heat equation
    /// \frac{dQ}{dt} = g \Delta T.
    /// If we need more precision in the future consider using a higher order integration scheme.
    /// If we need support for larger time steps in the future consider adding a method to split the time delta into several
    /// integration steps with adaptive step size.
    /// </remarks>
    [PublicAPI]
    public static float ConductHeatQuery(this ref ConductiveHeatContainer c, float temp, float deltaTime)
    {
        var dQ = c.ThermalConductance * (temp - c.Temperature) * deltaTime;
        var dQMax = Math.Abs(HeatContainerHelpers.ConductHeatToTempQuery(ref c, temp));

        // Clamp the transferred heat amount in case we are overshooting the equilibrium temperature because our time step was too large.
        return Math.Clamp(dQ, -dQMax, dQMax);
    }

    /// <summary>
    /// Calculates the amount of heat that would be conducted between two <see cref="IHeatContainer"/>s,
    /// given some time delta. Does not modify the containers.
    /// </summary>
    /// <param name="c1">The first <see cref="IHeatContainer"/> to conduct heat to.</param>
    /// <param name="c2">The second <see cref="IHeatContainer"/> to conduct heat to.</param>
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
    /// If we need more precision in the future consider using a higher order integration scheme.
    /// If we need support for larger time steps in the future consider adding a method to split the time delta into several
    /// integration steps with adaptive step size.
    /// </remarks>
    [PublicAPI]
    public static float ConductHeatQuery(this ref ConductiveHeatContainer c1, ref ConductiveHeatContainer c2, float deltaTime)
    {
        // The harmonic mean is used because conductance adds up inversely proportional.
        var g = 2f * c1.ThermalConductance * c2.ThermalConductance / (c1.ThermalConductance + c2.ThermalConductance);
        var dQ = g * (c2.Temperature - c1.Temperature) * deltaTime;
        var dQMax = Math.Min(Math.Abs(HeatContainerHelpers.ConductHeatToTempQuery(ref c1, c2.Temperature)),
            Math.Abs(HeatContainerHelpers.ConductHeatToTempQuery(ref c2, c1.Temperature)));

        // Clamp the transferred heat amount in case we are overshooting the equilibrium temperature because our time step was too large.
        return Math.Clamp(dQ, -dQMax, dQMax);
    }

    #endregion

    #region Divide

    /// <summary>
    /// Splits a <see cref="IHeatContainer"/> into two.
    /// </summary>
    /// <param name="c">The <see cref="IHeatContainer"/> to split. This will be modified to contain the remaining heat capacity.</param>
    /// <param name="cSplit">A <see cref="IHeatContainer"/> that will be modified to contain
    /// the specified fraction of the original container's heat capacity and the same temperature.</param>
    /// <param name="fraction">The fraction of the heat capacity to move to the new container. Clamped between 0 and 1.</param>
    [PublicAPI]
    public static void Split(this ref ConductiveHeatContainer c, ref ConductiveHeatContainer cSplit, float fraction = 0.5f)
    {
        fraction = Math.Clamp(fraction, 0f, 1f);
        var newHeatCapacity = c.HeatCapacity * fraction;
        var newThermalConductance = c.ThermalConductance * fraction;

        cSplit.HeatCapacity = newHeatCapacity;
        cSplit.ThermalConductance = newThermalConductance;
        cSplit.Temperature = c.Temperature;

        c.HeatCapacity -= newHeatCapacity;
        c.ThermalConductance -= newThermalConductance;
    }

    /// <summary>
    /// Divides a source <see cref="IHeatContainer"/> into a specified number of equal parts.
    /// </summary>
    /// <param name="c">The input <see cref="IHeatContainer"/> to split.</param>
    /// <param name="cFrac">A temporary working <see cref="IHeatContainer"/> that the method will use to
    /// fill the target array with dupes.</param>
    /// <param name="dividedArray">An array of <see cref="IHeatContainer"/>s equally split from the source <see cref="IHeatContainer"/>.
    /// This will be written to. This must be the same length as num.</param>
    /// <param name="num">The number of <see cref="IHeatContainer"/>s
    /// to split the source <see cref="IHeatContainer"/> into.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to divide the source container by zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of the divided array does not match the specified number of divisions.</exception>
    [PublicAPI]
    public static void Divide(this ConductiveHeatContainer c, ref ConductiveHeatContainer cFrac, ConductiveHeatContainer[] dividedArray, uint num)
    {
        ArgumentOutOfRangeException.ThrowIfZero(num);
        ArgumentOutOfRangeException.ThrowIfNotEqual(dividedArray.Length, (int)num);

        var fraction = 1f / num;
        Split(ref c, ref cFrac, fraction);

        for (var i = 0; i < num; i++)
        {
            dividedArray[i] = cFrac;
        }
    }

    #endregion

    #region Merge

    /// <summary>
    /// Merges two heat containers into one, conserving total internal energy.
    /// </summary>
    /// <param name="cA">The first <see cref="IHeatContainer"/> to merge. This will be modified to contain the merged result.</param>
    /// <param name="cB">The second <see cref="IHeatContainer"/> to merge.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the combined heat capacity of both containers is zero or negative.</exception>
    [PublicAPI]
    public static void Merge(this ref ConductiveHeatContainer cA, ref ConductiveHeatContainer cB)
    {
        var combinedHeatCapacity = cA.HeatCapacity + cB.HeatCapacity;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(combinedHeatCapacity);

        var temp = (cA.InternalEnergy + cB.InternalEnergy) / combinedHeatCapacity;
        cA.HeatCapacity = combinedHeatCapacity;
        cA.Temperature = temp;
        cA.ThermalConductance += cB.ThermalConductance;
    }

    /// <summary>
    /// Merges an array of <see cref="IHeatContainer"/>s into a single heat container, conserving total internal energy.
    /// </summary>
    /// <param name="cA">The first <see cref="IHeatContainer"/> to merge.
    /// This will be modified to contain the merged result.</param>
    /// <param name="cN">The array of <see cref="IHeatContainer"/>s to merge.</param>
    /// <param name="temp">A temporary <see cref="IHeatContainer"/> used to perform the merge.</param>
    [PublicAPI]
    public static void Merge(this ref ConductiveHeatContainer cA, ConductiveHeatContainer[] cN, ref ConductiveHeatContainer temp)
    {
        // merge the first array and then merge the result with cA to avoid alloc
        cN.Merge(ref temp);
        Merge(ref cA, ref temp);
    }

    /// <summary>
    /// Merges an array of <see cref="IHeatContainer"/>s into a single heat container, conserving total internal energy.
    /// </summary>
    /// <param name="cN">The array of <see cref="IHeatContainer"/>s to merge.</param>
    /// <param name="result">The modified <see cref="IHeatContainer"/> containing the merged result.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the combined heat capacity of all containers is zero or negative.</exception>
    [PublicAPI]
    public static void Merge(this ConductiveHeatContainer[] cN, ref ConductiveHeatContainer result)
    {
        var totalHeatCapacity = 0f;
        var totalEnergy = 0f;
        var totalThermalCondutance = 0f;

        foreach (var c in cN)
        {
            totalHeatCapacity += c.HeatCapacity;
            totalEnergy += c.InternalEnergy;
            totalThermalCondutance += c.ThermalConductance;
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalHeatCapacity);

        result.HeatCapacity = totalHeatCapacity;
        result.Temperature = totalEnergy / totalHeatCapacity;
        result.ThermalConductance = totalThermalCondutance;
    }

    #endregion
}
