using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Utils;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    /// <summary>
    /// Transfers an array of moles from second mixture to the first one,
    /// also accounting for transferred heat energy.
    /// </summary>
    /// <param name="m1"></param>
    /// <param name="m2"></param>
    /// <param name="firstMoles"></param>
    /// <param name="secondMoles"></param>
    /// <returns>The amount of transferred energy.</returns>
    [PublicAPI]
    public void TransferMoles<T1, T2>(ref T1 m1, ref T2 m2, Span<float> firstMoles, Span<float> secondMoles)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        TransferMoles(ref m1, ref m2, firstMoles, secondMoles, SharedPool);
    }

    [PublicAPI]
    public void TransferMoles<T1, T2>(ref T1 m1, ref T2 m2, Span<float> firstMoles, Span<float> secondMoles, IRobustArrayPool<float> pool)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var firstOldCapacity = GetHeatCapacity(ref m1, pool);
        var secondOldCapacity = GetHeatCapacity(ref m2, pool);
        var firstOldTemperature = m1.Temperature;
        var secondOldTemperature = m2.Temperature;

        // Separate positive and negative heat capacity change into 2 buffers, then sum them up.
        var buffer1 = pool.Rent();
        var buffer2 = pool.Rent();
        NumericsHelpers.Multiply(firstMoles, Prototypes.GasSpecificHeats, buffer1);
        NumericsHelpers.Multiply(secondMoles, Prototypes.GasSpecificHeats, buffer2);
        var heatCapacityToSharer = Math.Abs(NumericsHelpers.HorizontalAdd(buffer1));
        var heatCapacitySharerToThis = Math.Abs(NumericsHelpers.HorizontalAdd(buffer2));
        pool.Return(buffer1, true);
        pool.Return(buffer2, true);

        NumericsHelpers.Add(m1.Moles, secondMoles);
        NumericsHelpers.Add(m2.Moles, firstMoles);
        NumericsHelpers.Sub(m1.Moles, firstMoles);
        NumericsHelpers.Sub(m2.Moles, secondMoles);
        NumericsHelpers.Max(m1.Moles, 0f);
        NumericsHelpers.Max(m2.Moles, 0f);

        // Transfer of thermal energy (via changed heat capacity) between self and sharer:
        // T_new = W_old - W_removed + W_added
        m1.Temperature =
            (firstOldCapacity * m1.Temperature - heatCapacityToSharer * m1.Temperature + heatCapacitySharerToThis * secondOldTemperature)
            / GetHeatCapacity(ref m1, pool);
        m2.Temperature =
            (secondOldCapacity * m2.Temperature - heatCapacitySharerToThis * m2.Temperature + heatCapacityToSharer * firstOldTemperature)
            / GetHeatCapacity(ref m2, pool);
    }

    [PublicAPI]
    public void TransferMoles<T1, T2>(ref T1 m1, T2 m2, Span<float> firstMoles, Span<float> secondMoles)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        TransferMoles(ref m1, m2, firstMoles, secondMoles, SharedPool);
    }

    [PublicAPI]
    public void TransferMoles<T1, T2>(ref T1 m1, T2 m2, Span<float> firstMoles, Span<float> secondMoles, IRobustArrayPool<float> pool)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var firstOldCapacity = GetHeatCapacity(ref m1, pool);
        var secondOldTemperature = m2.Temperature;

        // Separate positive and negative heat capacity change into 2 buffers, then sum them up.
        var buffer1 = pool.Rent();
        var buffer2 = pool.Rent();
        NumericsHelpers.Multiply(firstMoles, Prototypes.GasSpecificHeats, buffer1);
        NumericsHelpers.Multiply(secondMoles, Prototypes.GasSpecificHeats, buffer2);
        var heatCapacityToSharer = Math.Abs(NumericsHelpers.HorizontalAdd(buffer1));
        var heatCapacitySharerToThis = Math.Abs(NumericsHelpers.HorizontalAdd(buffer2));
        pool.Return(buffer1, true);
        pool.Return(buffer2, true);

        NumericsHelpers.Add(m1.Moles, secondMoles);
        NumericsHelpers.Sub(m1.Moles, firstMoles);
        NumericsHelpers.Max(m1.Moles, 0f);

        // Transfer of thermal energy (via changed heat capacity) between self and sharer:
        // T_new = W_old - W_removed + W_added
        m1.Temperature =
            (firstOldCapacity * m1.Temperature - heatCapacityToSharer * m1.Temperature + heatCapacitySharerToThis * secondOldTemperature)
            / GetHeatCapacity(ref m1, pool);
    }
}
