using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using Content.Shared.Utils;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    public void ShareTiles(ref TileAtmos m1, TileAtmos m2, float cLength, float deltaTime, float k, IRobustArrayPool<float> pool)
    {
        var buffer1 = pool.Rent();
        var buffer2 = pool.Rent();
        ShareTilesQuery(ref m1.Mixture, ref m2.Mixture, cLength, deltaTime, k, buffer1, buffer2, pool);
        TransferMoles(ref m1.CachedMixture, m2.CachedMixture, buffer1, buffer2, pool);
        pool.Return(buffer1);
        pool.Return(buffer2);
    }

    public void ShareTilesQuery<T1, T2>(
        ref T1 m1,
        ref T2 m2,
        float cLength,
        float deltaTime,
        float k,
        Span<float> firstMoles,
        Span<float> secondMoles,
        IRobustArrayPool<float> pool)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var buffer1 = pool.Rent();
        var buffer2 = pool.Rent();
        // Partial pressure difference
        GetPartialPressures(ref m1, buffer1);
        GetPartialPressures(ref m2, buffer2);
        NumericsHelpers.Sub(buffer1, buffer2, firstMoles);
        NumericsHelpers.Sub(buffer2, buffer1, secondMoles);
        NumericsHelpers.Multiply(firstMoles, deltaTime * cLength * k);
        NumericsHelpers.Multiply(secondMoles, deltaTime * cLength * k);
        pool.Return(buffer1);
        pool.Return(buffer2);
    }

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
        // A safety mechanism against floating point errors
        // TODO find a faster method
        if (NumericsHelpers.HorizontalAdd(firstMoles) < SystemConstants.Epsilon
            || NumericsHelpers.HorizontalAdd(secondMoles) < SystemConstants.Epsilon)
            return;

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
        pool.Return(buffer1);
        pool.Return(buffer2);

        if (!m1.Immutable)
        {
            NumericsHelpers.Add(m1.Moles, secondMoles);
            NumericsHelpers.Sub(m1.Moles, firstMoles);
            NumericsHelpers.Max(m1.Moles, 0f);
        }

        if (!m2.Immutable)
        {
            NumericsHelpers.Add(m2.Moles, firstMoles);
            NumericsHelpers.Sub(m2.Moles, secondMoles);
            NumericsHelpers.Max(m2.Moles, 0f);
        }

        // Transfer of thermal energy (via changed heat capacity) between self and sharer:
        // T_new = W_old - W_removed + W_added
        m1.Temperature = m1.Immutable ? m1.Temperature :
            (firstOldCapacity * m1.Temperature - heatCapacityToSharer * m1.Temperature + heatCapacitySharerToThis * secondOldTemperature)
            / GetHeatCapacity(ref m1, pool);
        m2.Temperature = m2.Immutable ? m2.Temperature :
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
        pool.Return(buffer1);
        pool.Return(buffer2);

        if (!m1.Immutable)
        {
            NumericsHelpers.Add(m1.Moles, secondMoles);
            NumericsHelpers.Sub(m1.Moles, firstMoles);
            NumericsHelpers.Max(m1.Moles, 0f);
        }

        // Transfer of thermal energy (via changed heat capacity) between self and sharer:
        // T_new = W_old - W_removed + W_added
        /*m1.Temperature = m1.Immutable ? m1.Temperature :
            (firstOldCapacity * m1.Temperature - heatCapacityToSharer * m1.Temperature + heatCapacitySharerToThis * secondOldTemperature)
            / GetHeatCapacity(ref m1, pool);*/
    }
}
