using Robust.Shared.Serialization;

namespace Content.Shared.GameTicker;

/// <summary>
/// This is a very simple version of SS14's GameTicker that provides some basic utilities without the lobby.
/// </summary>
public abstract class SharedGameTicker : EntitySystem;

/// <summary>
/// Message sent to client that tells it to switch to GameplayState.
/// </summary>
[Serializable, NetSerializable]
public sealed class TickerJoinGameMessage : EntityEventArgs;
