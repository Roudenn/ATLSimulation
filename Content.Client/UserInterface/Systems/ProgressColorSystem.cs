using System.Numerics;

namespace Content.Client.UserInterface.Systems;

/// <summary>
/// This system handles getting an interpolated color based on the value of a cvar.
/// </summary>
public sealed class ProgressColorSystem : EntitySystem
{
    public Color GetProgressColor(float progress)
    {
        if (progress >= 1.0f)
        {
            return new Color(0f, 1f, 0f);
        }

        // lerp
        var hue = 5f / 18f * progress;
        return Color.FromHsv(new Vector4(hue, 1f, 0.75f, 1f));
    }
}
