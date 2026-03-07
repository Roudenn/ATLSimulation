using Content.Shared.Atmospherics.Systems;
using Content.Shared.Constants;
using Content.Shared.Temperature.HeatContainers;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmospherics.GasMixtures;

/// <summary>
/// A general-purpose container for gases.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
[Access(typeof(GasMixtureHelpers))]
public partial struct GasMixture : IRobustCloneable<GasMixture>
{
    /// <summary>
    /// Contains an amount of every single gas in moles.
    /// </summary>
    [DataField(customTypeSerializer: typeof(GasArraySerializer))]
    public float[] Moles;

    /// <summary>
    /// Heat container of the gas mixture.
    /// </summary>
    [DataField]
    public HeatContainer HeatContainer;

    /// <summary>
    /// Volume of the mixture's container.
    /// </summary>
    [DataField]
    public float Volume = 2500f;

    /// <summary>
    /// Total amount of moles of a gas mixture.
    /// Calculated dynamically when moles are added to the mixture.
    /// </summary>
    [ViewVariables]
    public float TotalMoles => this.GetTotalMolesQuery();

    /// <summary>
    /// Percentages of all moles in the gas mixture.
    /// Basically just <see cref="Moles"/> divided by <see cref="TotalMoles"/>
    /// </summary>
    [ViewVariables]
    public float[] MolesRatio => this.GetMolesRatioQuery();

    [ViewVariables]
    public float Pressure => this.GetPressureQuery();

    /// <summary>
    /// If true, any attempts to modify the mixture will be cancelled.
    /// Immutable fixtures are the main reason why first law of thermodynamics is technically violated.
    /// </summary>
    [DataField]
    public bool Immutable;

    /// <summary>
    /// Constructs a new gas mixture without any gas.
    /// </summary>
    public GasMixture(GasPrototypeManager manager, float volume, float temperature, bool immutable = false)
    {
        Moles = new float[manager.ArraySize];
        Volume = volume;
        HeatContainer = new HeatContainer(SystemConstants.MinimumHeatCapacity, temperature, 0f);
        Immutable = immutable;
    }

    /// <summary>
    /// Constructs a new gas mixture out of an array of moles and a heat container.
    /// </summary>
    /// <remarks>
    /// Ensure that the array size is correct and heat container has correct heat capacity relatively to the <see cref="Moles"/> array.
    /// </remarks>
    public GasMixture(float[] moles, float volume, HeatContainer container)
    {
        Moles = moles;
        Volume = volume;
        HeatContainer = container;
    }

    /// <summary>
    /// Constructs a new gas mixture based on the array of moles.
    /// </summary>
    public GasMixture(SharedAtmosphericsSystem system, float[] moles, float volume, float temperature)
    {
        Moles = moles;
        Volume = volume;
        HeatContainer = system.CreateGasHeatContainer(ref this, temperature);
    }

    public GasMixture(GasMixture g)
    {
        Moles = g.Moles;
        Volume = g.Volume;
        HeatContainer = g.HeatContainer;
        Immutable = g.Immutable;
    }

    public GasMixture Clone()
    {
        return new GasMixture(this);
    }
}
