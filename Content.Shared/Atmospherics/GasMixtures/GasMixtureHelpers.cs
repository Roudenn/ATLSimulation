using Content.Shared.Atmospherics.Systems;
using Content.Shared.Constants;
using Content.Shared.Temperature.HeatContainers;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.GasMixtures;

/// <summary>
/// Provides the most basic operations to work with a gas mixture.
/// Exists to guarantee that the total mass of gas in the simulation stays the same while it's fully isolated.
///
/// To get more convenient and safe functions to work with a gas mixture, use <see cref="SharedAtmosphericsSystem"/>'s API.
/// </summary>
public static partial class GasMixtureHelpers
{
    [PublicAPI]
    public static float GetMoles(this ref GasMixture m, int gasId)
    {
        return m.Moles[gasId];
    }

    [PublicAPI]
    public static float[] GetMolesRatioQuery(this ref GasMixture m)
    {
        var ratio = new float[m.Moles.Length]; // TODO this could be awful. Profile it!!!
        NumericsHelpers.Divide(m.Moles, GetTotalMolesQuery(ref m), ratio);
        return ratio;
    }

    [PublicAPI]
    public static float GetTotalMolesQuery(this ref GasMixture m)
        => NumericsHelpers.HorizontalAdd(m.Moles);

    [PublicAPI]
    public static void AddHeat(this ref GasMixture m, float energy)
    {
        if (m.Immutable)
            return;

        m.HeatContainer.AddHeat(energy);
    }

    /// <summary>
    /// Adds or removes moles of a gas inside the gas mixture, depending on the sign of the <see cref="moles"/> parameter.
    /// </summary>
    /// <param name="m"></param>
    /// <param name="gasId"></param>
    /// <param name="moles"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float AdjustMoles(this ref GasMixture m, int gasId, float moles)
    {
        if (m.Immutable)
            return 0f;

        m.Moles[gasId] += moles;
        return moles;
    }

    /// <summary>
    /// Marks the mixture as immutable, meaning that any changes to the mixture will be cancelled.
    /// </summary>
    /// <param name="m"></param>
    [PublicAPI]
    public static void MarkImmutable(this ref GasMixture m)
    {
        m.Immutable = true;
    }

    /// <summary>
    /// Sets new volume for this gas mixture.
    /// </summary>
    /// <param name="m"></param>
    /// <param name="volume"></param>
    [PublicAPI]
    public static void SetVolume(this ref GasMixture m, float volume)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(volume);
        m.Volume = volume;
    }

    [PublicAPI]
    public static float GetPressureQuery(this ref GasMixture m)
        => m.TotalMoles * m.HeatContainer.Temperature * PhysicalConstants.R / m.Volume;
}
