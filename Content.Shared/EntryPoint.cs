using System.Globalization;
using Content.Shared.Atmospherics;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Shared;
using Robust.Shared.Configuration;
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
    [Dependency] private readonly ILocalizationManager _localeManager = default!;
    //[Dependency] private readonly IConfigurationManager _configManager = default!;

    // IoC services shared between the client and the server go here...

    // See line 24. Controls the default game culture and language.
    // Robust calls this culture, but you might find it more fitting to call it the game
    // language. Robust doesn't support changing this mid-game. Load your config file early
    // if you want that.
    private const string Culture = "en-US";

    public override void PreInit()
    {
        Dependencies.InjectDependencies(this);

        _localeManager.LoadCulture(new CultureInfo(Culture));
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
            Dependencies.Resolve<GasPrototypeManager>().Register(tileDef);
        }

        Dependencies.Resolve<GasPrototypeManager>().Initialize();
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
            gas.AssignGasId(Dependencies.Resolve<GasPrototypeManager>()[gas.ID].GasId);
        }
    }
}
