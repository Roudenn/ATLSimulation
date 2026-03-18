using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Atmospherics.Systems;
using Content.Shared.Constants;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Atmospherics.Prototypes;

/// <summary>
/// A prototype for gas mixture composition and temperature.
/// Used by <see cref="SharedAtmosphericsSystem"/> in order to create <see cref="GasMixture"/> instances.
/// </summary>
[Prototype]
public sealed partial class GasMixturePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public float Temperature = PhysicalConstants.ROOM_TEMPERATURE;

    [DataField(customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, GasPrototype>))]
    public Dictionary<string, float> Moles = new();
}
