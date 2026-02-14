using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.GameTicking;
using Content.Shared.Mapping.Components;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking;

/// <inheritdoc/>
public sealed class GameTicker : SharedGameTicker
{
    [ViewVariables]
    public bool DummyTicker { get; private set; } = false;

    public static readonly EntProtoId ObserverEntity = "MobObserver";

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
    }

    public void PostInitialize()
    {
        RestartSimulationMap();
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
        if (_playerManager.SetAttachedEntity(session, entity))
        {
            PlayerJoinGame(session);
            Log.Info($"Successfully spawned and attached player {session.Name}");
        }
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
        // TODO map selection code
        var newMap = _mapSystem.CreateMap();
        EnsureComp<SimulationMapComponent>(newMap);
        Log.Info($"Created main simulation map {ToPrettyString(newMap)}");
        return newMap;
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
