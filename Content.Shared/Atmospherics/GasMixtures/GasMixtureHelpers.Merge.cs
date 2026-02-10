using System.Runtime.CompilerServices;
using Content.Shared.Temperature.HeatContainers;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Shared.Atmospherics.GasMixtures;

public static partial class GasMixtureHelpers
{
    /// <summary>
    /// Merges mixture m2 into mixture m1 by combining their moles, volume, and internal energy.
    /// </summary>
    /// <param name="m1">The mixture that initiated merge.</param>
    /// <param name="m2">The second mixture to merge the gas with.</param>
    [PublicAPI]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Merge(this ref GasMixture m1, ref GasMixture m2)
    {
        DebugTools.Assert(m1.Immutable, "The mixture targeted for a merge was marked as immutable!");
        
        var combinedVolume = m1.Volume + m2.Volume;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(combinedVolume);
        NumericsHelpers.Add(m1.Moles, m2.Moles);
        m1.HeatContainer.Merge(m2.HeatContainer);
        
        m1 = new GasMixture(m1.Moles, combinedVolume, m1.HeatContainer);
    }
    
    /// <summary>
    /// Merges mixture m2 into mixture m1 by combining their moles, volume, and internal energy.
    /// </summary>
    /// <param name="m1">The mixture that initiated merge.</param>
    /// <param name="m2">The second mixture to merge the gas with.</param>
    [PublicAPI]
    public static void MergeWithoutVolume(this ref GasMixture m1, ref GasMixture m2)
    {
        DebugTools.Assert(m1.Immutable, "The mixture targeted for a merge was marked as immutable!");
        
        NumericsHelpers.Add(m1.Moles, m2.Moles);
        m1.HeatContainer.Merge(m2.HeatContainer);
        
        m1 = new GasMixture(m1.Moles, m1.Volume, m1.HeatContainer);
    }
}