using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Atmospherics;

[Virtual]
public class GasPrototypeManager : IGasPrototypeManager
{
    protected readonly List<GasPrototype> GasDefs;
    private readonly Dictionary<string, GasPrototype> _gasNames;

    /// <summary>
    /// Default Constructor.
    /// </summary>
    public GasPrototypeManager()
    {
        GasDefs = new List<GasPrototype>();
        _gasNames = new Dictionary<string, GasPrototype>();
    }

    public virtual void Initialize()
    {
    }

    public void Register(GasPrototype gasDef)
    {
        var name = gasDef.ID;
        if (_gasNames.ContainsKey(name))
        {
            throw new ArgumentException("Another tile definition or alias with the same name has already been registered.", nameof(gasDef));
        }

        var id = checked((byte) GasDefs.Count);
        gasDef.AssignGasId(id);
        GasDefs.Add(gasDef);
        _gasNames[name] = gasDef;
    }

    public int Count => GasDefs.Count;
    
    public int ArraySize => MathHelper.NextMultipleOf(Count, 4);

    public GasPrototype this[string name] => _gasNames[name];

    public GasPrototype this[int id] => GasDefs[id];

    public bool TryGetDefinition(string name, [NotNullWhen(true)] out GasPrototype? definition)
    {
        return _gasNames.TryGetValue(name, out definition);
    }

    public bool TryGetDefinition(int id, [NotNullWhen(true)] out GasPrototype? definition)
    {
        if (id >= GasDefs.Count)
        {
            definition = null;
            return false;
        }

        definition = GasDefs[id];
        return true;
    }
    
    public IEnumerator<GasPrototype> GetEnumerator()
    {
        return GasDefs.GetEnumerator();
    }
    
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
