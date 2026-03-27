using System.Diagnostics.CodeAnalysis;
using Content.Shared.Utils;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory : IGasMixtureFactory
{
    public RobustArrayPool<float> Pool = default!;
    public GasPrototypesBuffer Prototypes = default!;

    private readonly List<GasPrototype> _gasDefs;
    private readonly Dictionary<string, GasPrototype> _gasNames;

    public GasMixtureFactory()
    {
        _gasDefs = new List<GasPrototype>();
        _gasNames = new Dictionary<string, GasPrototype>();
    }

    public void Initialize()
    {
        InitializeGases();
    }

    public void InitializeGases()
    {
        Pool = new RobustArrayPool<float>(ArraySize, 16);
        Prototypes = new GasPrototypesBuffer(ArraySize);

        for (var i = 0; i < Count; i++)
        {
            Prototypes.RegisterPrototype(this[i]);
        }
    }

    public void Register(GasPrototype gasDef)
    {
        var name = gasDef.ID;
        if (_gasNames.ContainsKey(name))
        {
            throw new ArgumentException("Another gas definition or alias with the same name has already been registered.", nameof(gasDef));
        }

        var id = checked((byte) _gasDefs.Count);
        gasDef.AssignGasId(id);
        _gasDefs.Add(gasDef);
        _gasNames[name] = gasDef;
    }

    public int Count => _gasDefs.Count;

    public int ArraySize => MathHelper.NextMultipleOf(Count, 4);

    public GasPrototype this[string name] => _gasNames[name];

    public GasPrototype this[int id] => _gasDefs[id];

    public bool TryGetDefinition(string name, [NotNullWhen(true)] out GasPrototype? definition)
    {
        return _gasNames.TryGetValue(name, out definition);
    }

    public bool TryGetDefinition(int id, [NotNullWhen(true)] out GasPrototype? definition)
    {
        if (id >= _gasDefs.Count)
        {
            definition = null;
            return false;
        }

        definition = _gasDefs[id];
        return true;
    }

    public IEnumerator<GasPrototype> GetEnumerator()
    {
        return _gasDefs.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
