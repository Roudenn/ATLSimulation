using Content.Server.IoC;
using JetBrains.Annotations;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;

// DEVNOTE: Games that want to be on the hub can change their namespace prefix in the "manifest.yml" file.
namespace Content.Server;

[UsedImplicitly]
public sealed class EntryPoint : GameServer
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    
    public override void PreInit()
    {
        ServerContentIoC.Register(Dependencies);
    }

    public override void Init()
    {
        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
        
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

        // DEVNOTE: This is generally where you'll be setting up the IoCManager further.
    }

    public override void PostInit()
    {
        base.PostInit();
        // DEVNOTE: Can also initialize IoC stuff more here.
    }
}