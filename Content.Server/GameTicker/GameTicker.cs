using System.Numerics;
using Content.Shared.GameTicker;
using Content.Shared.Mapping.Components;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.GameTicker;

/// <inheritdoc/>
public sealed class GameTicker : SharedGameTicker
{
    public static readonly EntProtoId ObserverEntity = "MobObserver";
    
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
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
                // Ensure that everything is here
                var coords = new EntityCoordinates(EnsureMainMap(), Vector2.Zero);
                var entity = Spawn(ObserverEntity, coords);
                
                // Spawn our player
                if (_playerManager.SetAttachedEntity(session, entity))
                    PlayerJoinGame(session);
                
                break;
            case SessionStatus.Disconnected:
                break;
        }
    }

    private EntityUid EnsureMainMap()
    {
        var query = EntityQueryEnumerator<SimulationMapComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            return uid;
        }

        var newMap = _mapSystem.CreateMap();
        EnsureComp<SimulationMapComponent>(newMap);
        return newMap;
    }
}