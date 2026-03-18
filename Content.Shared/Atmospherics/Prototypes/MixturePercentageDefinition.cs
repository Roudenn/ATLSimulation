using Content.Shared.Atmospherics.Factory;
using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Atmospherics.Prototypes;

public sealed partial class MixturePercentageDefinition : MixtureDefinition
{
    [DataField]
    public float Volume = 1f;

    [DataField]
    public float Temperature = PhysicalConstants.ROOM_TEMPERATURE;

    [DataField]
    public float Pressure = 101.25f;

    [DataField(customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, GasPrototype>))]
    public Dictionary<string, float> Percentages = new();

    public override IGasMixture CreateMixture(IGasMixtureFactory factory, IPrototypeManager protoMan, float? volume = null)
    {
        var array = new float[factory.ArraySize];
        foreach (var (id, value) in Percentages)
        {
            if (!protoMan.Resolve<GasPrototype>(id, out var gasProto))
                continue;

            var totalMoles = Pressure * (volume ?? Volume) / (PhysicalConstants.R * Temperature) * 1000f;
            array[gasProto.GasId] = totalMoles * value;
        }

        return new GasMixture(array, volume ?? Volume, Temperature);
    }
}
