using Content.Server.GameTicking;
using Content.Server.IoC;
using Content.Shared.GameCVars;
using Content.Shared.Localizations;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;

// DEVNOTE: Games that want to be on the hub can change their namespace prefix in the "manifest.yml" file.
namespace Content.Server;

[UsedImplicitly]
public sealed class EntryPoint : GameServer
{
    private const string ConfigPresetsDir = "/ConfigPresets/";

    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly IEntitySystemManager _entSys = default!;
    [Dependency] private readonly ContentLocalizationManager _loc = default!;

    public override void PreInit()
    {
        ServerContentIoC.Register(Dependencies);
    }

    public override void Init()
    {
        base.Init();
        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);

        _loc.Initialize();

        LoadConfigPresets(_cfg, _res, _log.GetSawmill("configpreset"));

        _componentFactory.DoAutoRegistrations();

        foreach (var ignoreName in IgnoredComponents.List)
        {
            _componentFactory.RegisterIgnore(ignoreName);
        }

        foreach (var ignoreName in IgnoredPrototypes.List)
        {
            _prototypeManager.RegisterIgnore(ignoreName);
        }

        _componentFactory.GenerateNetIds();
    }

    public override void PostInit()
    {
        base.PostInit();

        _entSys.GetEntitySystem<GameTicker>().PostInitialize();
    }

    private static void LoadConfigPresets(IConfigurationManager cfg, IResourceManager res, ISawmill sawmill)
    {
        var presets = cfg.GetCVar(GameConfigVars.ConfigPresets);
        if (presets == "")
            return;

        foreach (var preset in presets.Split(','))
        {
            var path = $"{ConfigPresetsDir}{preset}.toml";
            if (!res.TryContentFileRead(path, out var file))
            {
                sawmill.Error("Unable to load config preset {Preset}!", path);
                continue;
            }

            cfg.LoadDefaultsFromTomlStream(file);
            sawmill.Info("Loaded config preset: {Preset}", path);
        }
    }
}
