using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

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

    /// <summary>
    /// Color of this gas that is used in UIs.
    /// </summary>
    [DataField]
    public Color Color = Color.White;

    /// <summary>
    /// Sprite of this gas when it's placed inside <see cref="TileAtmosphere"/>.
    /// </summary>
    [DataField]
    public SpriteSpecifier? TileSprite;

    // for now a byte because an array is used to store gases and anything more than 255 will allocate gazzilion memory
    [ViewVariables]
    public byte GasId { get; private set; }

    [ViewVariables]
    public float PrandtlNumber => SpecificMolarHeat * Viscosity / (ThermalConductivity * MolarMass) / 1000f;

    /// <summary>
    /// The literal name of the material that can be seen by users.
    /// </summary>
    [ViewVariables]
    public LocId Name => $"gas-name-{ID}";

    /// <summary>
    /// The shortened name of the material that represents its chemical formula.
    /// </summary>
    [ViewVariables]
    public LocId Abbreviation => $"gas-abbreviation-{ID}";

    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <inheritdoc />
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<GasPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc />
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    public void AssignGasId(byte id)
    {
        GasId = id;
    }
}
