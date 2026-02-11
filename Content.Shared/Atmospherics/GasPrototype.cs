using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Atmospherics;

[Prototype]
public sealed partial class GasPrototype : IPrototype, IInheritingPrototype
{
    /// <summary>
    /// The literal name of the material that can be seen by users.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;
    
    /// <summary>
    /// The shortened name of the material that represents its chemical formula.
    /// </summary>
    [DataField(required: true)]
    public LocId Abbreviation;
    
    [DataField(required: true)]
    public float MolarMass;
    
    [DataField(required: true)]
    public float SpecificMolarHeat;
    
    [DataField(required: true)]
    public float ThermalConductivity;
    
    [DataField(required: true)]
    public float Viscosity;
    
    // for now a byte because an array is used to store gases and anything more than 255 will allocate gazzilion memory
    [ViewVariables]
    public byte GasId { get; private set; } 
    
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