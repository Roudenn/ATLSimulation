using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Shared.ContentPack;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

// DEVNOTE: Games that want to be on the hub can change their namespace prefix in the "manifest.yml" file.
namespace Content.Shared;

[UsedImplicitly]
public sealed class EntryPoint : GameShared
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;

    public override void PreInit()
    {
        Dependencies.InjectDependencies(this);
    }

    public override void PostInit()
    {
        _prototypeManager.PrototypesReloaded += PrototypeReload;
        InitTileDefinitions();
        InitGasDefinitions();
    }

    private void InitTileDefinitions()
    {
        // Register space first because I'm a hard coding hack.
        var spaceDef = _prototypeManager.Index<ContentTileDefinition>(ContentTileDefinition.SpaceID);

        _tileDefinitionManager.Register(spaceDef);

        var prototypeList = new List<ContentTileDefinition>();
        foreach (var tileDef in _prototypeManager.EnumeratePrototypes<ContentTileDefinition>())
        {
            if (tileDef.ID == ContentTileDefinition.SpaceID)
                continue;

            prototypeList.Add(tileDef);
        }

        // Sort ordinal to ensure it's consistent client and server.
        // So that tile IDs match up.
        prototypeList.Sort((a, b) => string.Compare(a.ID, b.ID, StringComparison.Ordinal));

        foreach (var tileDef in prototypeList)
        {
            _tileDefinitionManager.Register(tileDef);
        }

        _tileDefinitionManager.Initialize();
    }

    private void InitGasDefinitions()
    {
        var prototypeList = new List<GasPrototype>();
        foreach (var tileDef in _prototypeManager.EnumeratePrototypes<GasPrototype>())
        {
            prototypeList.Add(tileDef);
        }

        // Sort ordinal to ensure it's consistent client and server.
        // So that tile IDs match up.
        prototypeList.Sort((a, b) => string.Compare(a.ID, b.ID, StringComparison.Ordinal));

        foreach (var tileDef in prototypeList)
        {
            Dependencies.Resolve<GasMixtureFactory>().Register(tileDef);
        }

        Dependencies.Resolve<GasMixtureFactory>().Initialize();
    }

    private void PrototypeReload(PrototypesReloadedEventArgs obj)
    {
        // Need to re-allocate tiledefs due to how prototype reloads work
        foreach (var def in _prototypeManager.EnumeratePrototypes<ContentTileDefinition>())
        {
            def.AssignTileId(_tileDefinitionManager[def.ID].TileId);
        }

        foreach (var gas in _prototypeManager.EnumeratePrototypes<GasPrototype>())
        {
            gas.AssignGasId(Dependencies.Resolve<GasMixtureFactory>()[gas.ID].GasId);
        }
    }
}
