using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Materials;

[Prototype]
public sealed partial class MaterialPrototype : IPrototype, IInheritingPrototype
{
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
    
    /// <summary>
    /// The literal name of the material that is displayed in UIs.
    /// </summary>
    [ViewVariables]
    public LocId Name => $"material-name-{ID}";
    
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdSerializer<MaterialPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc/>
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }
}
