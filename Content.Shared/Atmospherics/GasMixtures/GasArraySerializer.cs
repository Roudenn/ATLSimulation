using Content.Shared.Atmospherics.Factory;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared.Atmospherics.GasMixtures;

/// <summary>
/// Serializer for the gas mixture's main array.
/// </summary>
public sealed class GasArraySerializer : ITypeSerializer<float[], SequenceDataNode>, ITypeSerializer<float[], MappingDataNode>
{
    public ValidationNode Validate(ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        var list = new List<ValidationNode>();
        foreach (var elem in node.Sequence)
        {
            list.Add(serializationManager.ValidateNode<float>(elem, context));
        }

        return new ValidatedSequenceNode(list);
    }

    public float[] Read(ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<float[]>? instanceProvider = null)
    {
        var gasMan = dependencies.Resolve<GasMixtureFactory>();
        var list = instanceProvider != null ? instanceProvider() : new float[gasMan.Count];

        for (var i = 0; i < node.Sequence.Count; i++)
        {
            list[i] = serializationManager.Read<float>(node.Sequence[i], hookCtx, context);
        }

        return list;
    }

    public ValidationNode Validate(ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        var gasMan = dependencies.Resolve<GasMixtureFactory>();
        var dict = new Dictionary<ValidationNode, ValidationNode>();

        foreach (var (key, value) in node.Children)
        {
            ValidationNode keyNode = gasMan.TryGetDefinition(key, out _)
                ? new ValidatedValueNode(node.GetKeyNode(key))
                : new ErrorNode(node.GetKeyNode(key), $"Failed to parse Gas definition: {key}");

            dict.Add(keyNode, serializationManager.ValidateNode<float>(value, context));
        }

        return new ValidatedMappingNode(dict);
    }

    public float[] Read(ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<float[]>? instanceProvider = null)
    {
        var gasMan = dependencies.Resolve<GasMixtureFactory>();
        var list = instanceProvider != null ? instanceProvider() : new float[gasMan.Count];

        foreach (var (gas, value) in node.Children)
        {
            // In the event that an invalid gas got serialized into something,
            // we simply ignore it and continue reading.
            // Errors should already be caught by Validate().
            if (!gasMan.TryGetDefinition(gas, out var gasProto))
                continue;

            list[gasProto.GasId] = serializationManager.Read<float>(value, hookCtx, context);
        }

        return list;
    }

    public DataNode Write(ISerializationManager serializationManager,
        float[] value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        var gasMan = dependencies.Resolve<GasMixtureFactory>();
        var mapping = new MappingDataNode();

        for (var i = 0; i < gasMan.Count; i++)
        {
            if (value[i] == 0f)
                continue; // Skip empty entries, they don't matter.

            var gas = gasMan[i];
            mapping.Add(gas.ID, serializationManager.WriteValue(value[i], alwaysWrite, context));
        }

        return mapping;
    }
}
