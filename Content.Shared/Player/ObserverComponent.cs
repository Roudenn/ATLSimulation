using Robust.Shared.GameStates;

namespace Content.Shared.Player;

/// <summary>
/// Marks this entity as an observer, so it will delete after a player was detached from it.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ObserverComponent : Component;
