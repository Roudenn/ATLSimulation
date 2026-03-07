using System.Numerics;

namespace Content.Client.UserInterface.Systems;

public static class ProgressColorHelpers
{
    public static Color GetProgressColor(float progress)
    {
        if (progress >= 1.0f)
        {
            return new Color(0f, 1f, 0f);
        }

        // lerp
        var hue = 5f / 18f * progress;
        return Color.FromHsv(new Vector4(hue, 1f, 0.75f, 1f));
    }

    /// <summary>
    /// Simple yellow -> orange -> red gradient.
    /// </summary>
    public static Color GradientWarm(float value, float min, float max)
    {
        // map min to 1, max to 0
        value = (value - min) / (max - min);
        return value < 0.5f
            ? Color.InterpolateBetween(Color.Yellow, Color.Orange, value * 2)
            : Color.InterpolateBetween(Color.Orange, Color.Red, (value - 0.5f) * 2);
    }

    /// <summary>
    /// Simple green -> blue -> violet gradient.
    /// </summary>
    public static Color GradientCold(float value, float min, float max)
    {
        // map min to 1, max to 0
        value = (value - min) / (max - min);
        return value < 0.5f
            ? Color.InterpolateBetween(Color.Green, Color.Blue, value * 2)
            : Color.InterpolateBetween(Color.Blue, Color.Violet, (value - 0.5f) * 2);
    }
}
