using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Atmospherics.Prototypes;

public sealed partial class MixtureCompositionDefinition : MixtureDefinition
{
    [DataField]
    public float Volume = 1f;

    [DataField]
    public float Temperature = PhysicalConstants.ROOM_TEMPERATURE;

    [DataField(customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, GasPrototype>))]
    public Dictionary<string, float> Moles = new();

    public override IGasMixture CreateMixture(IGasMixtureFactory factory, IPrototypeManager protoMan, float? volume = null)
    {
        var array = new float[factory.ArraySize];
        foreach (var (id, value) in Moles)
        {
            if (!protoMan.Resolve<GasPrototype>(id, out var gasProto))
                continue;

            array[gasProto.GasId] = value;
        }

        return new GasMixture(array, volume ?? Volume, Temperature);
    }
}
