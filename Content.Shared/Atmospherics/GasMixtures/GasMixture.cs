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
    public float[] Moles; // TODO find a better way to save memory and at the same time not kill GC performance
    
    /// <summary>
    /// Percentages of all moles in the gas mixture.
    /// Basically just <see cref="Moles"/> divided by <see cref="TotalMoles"/>
    /// </summary>
    [ViewVariables]
    public float[] MolesRatio;
    
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
    public float TotalMoles;
    
    [ViewVariables]
    public float Pressure;
    
    /// <summary>
    /// If true, any attempts to modify the mixture will be cancelled.
    /// Immutable fixtures are the main reason why first law of thermodynamics is technically violated.
    /// </summary>
    [DataField]
    public bool Immutable;
    
    /// <summary>
    /// If true, the tile is considered to be spaced, allowing it to have special properties.
    /// </summary>
    [DataField]
    public bool IsSpace;
    
    /// <summary>
    /// Constructs a new gas mixture without any gas.
    /// </summary>
    public GasMixture(IGasPrototypeManager manager, float volume, float temperature)
    {
        Moles = new float[manager.ArraySize];
        MolesRatio = new float[manager.ArraySize];
        Volume = volume;
        HeatContainer = new HeatContainer(SystemConstants.MinimumHeatCapacity, temperature);
    }
    
    /// <summary>
    /// Constructs a new gas mixture based on the array of moles.
    /// </summary>
    /// <remarks>
    /// ENSURE THAT THE ARRAY SIZE IS EQUAL TO THE ONE IN THE GAS PROTOTYPE MANAGER!
    /// </remarks>
    public GasMixture(SharedAtmosphericsSystem system, float[] moles, float volume, float temperature)
    {
        Moles = moles;
        MolesRatio = this.CalculateMolesRatio();
        Volume = volume;
        HeatContainer = new HeatContainer(this.CalculateHeatCapacity(ref system.GasSpecificHeats), temperature);
    }
    
    public GasMixture(GasMixture g)
    {
        Moles = g.Moles;
        MolesRatio = g.MolesRatio;
        Volume = g.Volume;
        HeatContainer = g.HeatContainer;
    }
    
    public GasMixture Clone()
    {
        return new GasMixture(this);
    }
}
