using Content.Client.Gameplay;
using Content.Shared.GameTicking;
using Robust.Client.State;

namespace Content.Client.GameTicking;

/// <inheritdoc/>
public sealed class GameTicker : SharedGameTicker
{
    [Dependency] private readonly IStateManager _stateManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<TickerJoinGameMessage>(JoinGame);
    }

    private void JoinGame(TickerJoinGameMessage message)
    {
        _stateManager.RequestStateChange<GameplayState>();
    }
}
