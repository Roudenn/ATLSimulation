using Content.Shared.Atmospherics.GasMixtures;
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
    /// <param name="moles"></param>
    /// <returns>The amount of transferred energy.</returns>
    [PublicAPI]
    public void TransferMoles<T1, T2>(ref T1 m1, ref T2 m2, float[] moles)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var firstOldCapacity = GetHeatCapacity(ref m1);
        var secondOldCapacity = GetHeatCapacity(ref m2);
        var firstOldTemperature = m1.Temperature;
        var secondOldTemperature = m2.Temperature;

        // Separate positive and negative heat capacity change into 2 buffers, then sum them up.
        NumericsHelpers.Multiply(moles, GasSpecificHeats, GasBuffer1);
        NumericsHelpers.Max(GasBuffer1, 0f, GasBuffer2);
        NumericsHelpers.Min(GasBuffer1, 0f, GasBuffer3);

        var heatCapacityToSharer = Math.Abs(NumericsHelpers.HorizontalAdd(GasBuffer2));
        var heatCapacitySharerToThis = Math.Abs(NumericsHelpers.HorizontalAdd(GasBuffer3));
        ClearBuffer(ref GasBuffer1, ref GasBuffer2, ref GasBuffer3);

        NumericsHelpers.Sub(m1.Moles, moles);
        NumericsHelpers.Add(m2.Moles, moles);
        NumericsHelpers.Max(m1.Moles, 0f);
        NumericsHelpers.Max(m2.Moles, 0f);

        // Transfer of thermal energy (via changed heat capacity) between self and sharer:
        // T_new = W_old - W_removed + W_added
        m1.Temperature = ((firstOldCapacity * m1.Temperature) - (heatCapacityToSharer * m1.Temperature) + (heatCapacitySharerToThis * secondOldTemperature)) / GetHeatCapacity(ref m1);
        m2.Temperature = ((secondOldCapacity * m2.Temperature) - (heatCapacitySharerToThis * m2.Temperature) + (heatCapacityToSharer * firstOldTemperature)) / GetHeatCapacity(ref m2);
    }
}
