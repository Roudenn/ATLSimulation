using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server.Statistics;
using Content.Shared.GameCVars;
using Content.Shared.GameTicking;
using Content.Shared.Maps;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking;

/// <inheritdoc/>
public sealed class GameTicker : SharedGameTicker
{
    [ViewVariables]
    public string? CurrentSimulationMap;

    public static readonly EntProtoId ObserverEntity = "MobObserver";

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IConfigurationManager _configMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    public override void Initialize()
    {
        base.Initialize();
        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
        SubscribeLocalEvent<GetStatisticsEvent>(OnGetStats);
        Subs.CVar(_configMan, GameConfigVars.GameDefaultMap, s => CurrentSimulationMap = s == string.Empty ? null : s, true);
    }

    public void PostInitialize()
    {
        RestartSimulationMap();
    }

    private void OnGetStats(ref GetStatisticsEvent ev)
    {
        ev.Stats.CurrentMap = CurrentSimulationMap ?? "Empty";
    }

    public void PlayerJoinGame(ICommonSession session)
    {
        RaiseNetworkEvent(new TickerJoinGameMessage(), session.Channel);
    }

    private void PlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        var session = args.Session;

        switch (args.NewStatus)
        {
            case SessionStatus.Connected:
                // Make the player actually join the game.
                // timer time must be > tick length
                Timer.Spawn(0, () => _playerManager.JoinGame(args.Session));
                break;
            case SessionStatus.InGame:
                SpawnPlayer(session);
                break;
            case SessionStatus.Disconnected:
                break;
        }
    }

    private void SpawnPlayer(ICommonSession session)
    {
        // Ensure that everything is here
        var coords = new EntityCoordinates(EnsureMainMap(), Vector2.Zero);
        var entity = Spawn(ObserverEntity, coords);

        // Spawn our player
        if (!_playerManager.SetAttachedEntity(session, entity))
            return;

        PlayerJoinGame(session);
        Log.Info($"Successfully spawned and attached player {session.Name}");
    }

    private bool TryGetMainMap([NotNullWhen(true)] out EntityUid? map)
    {
        map = null;
        var query = EntityQueryEnumerator<SimulationMapComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            map = uid;
            return true;
        }

        return false;
    }

    private EntityUid EnsureMainMap()
    {
        if (TryGetMainMap(out var map))
            return map.Value;

        return CreateMainMap();
    }

    private EntityUid CreateMainMap()
    {
        var mapProto = SelectMap();
        if (mapProto.Path == null
            || !_mapLoader.TryLoadMap(mapProto.Path.Value, out var map, out _, new DeserializationOptions() { InitializeMaps = true}))
        {
            var newMap = _mapSystem.CreateMap(); // Fallback
            map = (newMap, Comp<MapComponent>(newMap));
        }

        EnsureComp<SimulationMapComponent>(map.Value);
        Log.Info($"Created main simulation map {ToPrettyString(map.Value)}");
        return map.Value.Owner;
    }

    private SimulationMapPrototype SelectMap()
    {
        if (CurrentSimulationMap != null)
            return _protoMan.Index<SimulationMapPrototype>(CurrentSimulationMap);

        var maps = _protoMan.EnumeratePrototypes<SimulationMapPrototype>().ToList();
        var map = _random.Pick(maps);
        CurrentSimulationMap = map.ID;
        return map;
    }

    private void SpawnAllPlayers()
    {
        Log.Info("Spawning all players...");
        foreach (var session in _playerManager.Sessions)
        {
            SpawnPlayer(session);
        }
    }

    public void RestartSimulationMap()
    {
        Log.Info("Restarting the simulation map!");
        EntityManager.FlushEntities();
        CreateMainMap();
        SpawnAllPlayers();
    }
}
