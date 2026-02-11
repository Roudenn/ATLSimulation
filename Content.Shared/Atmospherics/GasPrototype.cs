using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Atmospherics;

[Prototype]
public sealed partial class GasPrototype : IPrototype, IInheritingPrototype
{
    /// <summary>
    /// Molar mass in g/mol.
    /// </summary>
    [DataField(required: true)]
    public float MolarMass;
    
    /// <summary>
    /// Specific molar heat in J/(mol·K).
    /// </summary>
    [DataField(required: true)]
    public float SpecificMolarHeat;
    
    /// <summary>
    /// Thermal conductivity in W/(m·K).
    /// </summary>
    [DataField(required: true)]
    public float ThermalConductivity;
    
    /// <summary>
    /// Dynamic viscosity in µPa·s.
    /// </summary>
    [DataField(required: true)]
    public float Viscosity;
    
    // for now a byte because an array is used to store gases and anything more than 255 will allocate gazzilion memory
    [ViewVariables]
    public byte GasId { get; private set; }
    
    /// <summary>
    /// The literal name of the material that can be seen by users.
    /// </summary>
    [ViewVariables]
    public LocId Name => $"gas-name-{ID}";
    
    /// <summary>
    /// The shortened name of the material that represents its chemical formula.
    /// </summary>
    [ViewVariables]
    public LocId Abbreviation => "gas-abbreviation-" + ID.ToLower();
    
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdSerializer<GasPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc/>
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }
    
    public void AssignGasId(byte id)
    {
        GasId = id;
    }
}
