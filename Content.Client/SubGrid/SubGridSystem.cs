using Content.Shared.Subgrid.Systems;
using Robust.Client.Graphics;
using Robust.Client.Input;

namespace Content.Client.Subgrid;

public sealed class SubGridSystem : SharedSubGridSystem
{
    [Dependency] private readonly IInputManager _inputMan = default!;
    [Dependency] private readonly IEyeManager _eyeMan = default!;


}
