using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Materials;

[Prototype]
public sealed class MaterialPrototype : IPrototype, IInheritingPrototype
{
    /// <summary>
    /// The literal name of the material that can be seen by users.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;
    
    /// <summary>
    /// Density in kg/m3.
    /// </summary>
    [DataField(required: true)]
    public float Density;
    
    /// <summary>
    /// Specific heat capacity in J / (kg * K).
    /// </summary>
    [DataField(required: true)]
    public float SpecificHeatCapacity;
    
    /// <summary>
    /// Thermal conductivity in W / (m * K).
    /// </summary>
    [DataField(required: true)]
    public float ThermalConductivity;
    
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdSerializer<MaterialPrototype>))]
    public string[]? Parents { get; }

    /// <inheritdoc/>
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }
}
