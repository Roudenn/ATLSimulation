using Content.Shared.Atmospherics.GasMixtures;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    /// <summary>
    /// Adds or removes moles of a gas inside the gas mixture, depending on the sign of the <see cref="moles"/> parameter.
    /// </summary>
    /// <param name="m">Gas mixture to add the gas to.</param>
    /// <param name="gasProtoId">Prototype ID of the gas.</param>
    /// <param name="moles">Amount of gas to add.</param>
    [PublicAPI]
    public void AddMoles<T>(ref T m, ProtoId<GasPrototype> gasProtoId, float moles) where T : IGasMixture
    {
        var gasId = this[gasProtoId].GasId;
        m.Moles[gasId] += moles;
    }

    /// <summary>
    /// Adds or removes moles of a gas inside the gas mixture, depending on the sign of the <see cref="moles"/> parameter.
    /// </summary>
    /// <param name="m">Gas mixture to add the gas to.</param>
    /// <param name="gasId">ID of the gas.</param>
    /// <param name="moles">Amount of gas to add.</param>
    [PublicAPI]
    public void AddMoles<T>(ref T m, int gasId, float moles) where T : IGasMixture
    {
        m.Moles[gasId] += moles;
    }

    /// <summary>
    /// Adds or removes moles of a gas inside the gas mixture.
    /// </summary>
    /// <param name="m">Gas mixture to add the gas to.</param>
    /// <param name="moles">Amounts of gases to add.</param>
    [PublicAPI]
    public void AddMoles<T>(ref T m, ref float[] moles) where T : IGasMixture
    {
        NumericsHelpers.Add(m.Moles, moles);
    }

    /// <summary>
    /// Sets new volume for this gas mixture.
    /// </summary>
    /// <param name="m">Gas mixture to change the volume of.</param>
    /// <param name="volume">New volume for the gas mixture.</param>
    [PublicAPI]
    public void SetVolume<T>(ref T m, float volume) where T : IGasMixture
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(volume);
        m.Volume = volume;
    }
}
