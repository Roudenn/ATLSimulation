using System.Numerics;
using Robust.Shared.Collections;

namespace Content.Shared.Maths;

public static partial class Box2Extensions
{
    /// <summary>
    /// Splits the box into an array of boxes split by some other box.
    /// Similar to Intersect, but it also returns the remainders.
    /// </summary>
    public static Box2[] AABBIntersection(this Box2 box, Box2 other)
    {
        float interLeft = Math.Max(box.BottomLeft.X, other.BottomLeft.X);
        float interBottom = Math.Max(box.BottomLeft.Y, other.BottomLeft.Y);
        float interRight = Math.Min(box.TopRight.X, other.TopRight.X);
        float interTop = Math.Min(box.TopRight.Y, other.TopRight.Y);

        if (interLeft >= interRight || interBottom >= interTop)
            return new[] { box };

        var results = new ValueList<Box2>(5);
        results.Add(new Box2(new Vector2(interLeft, interBottom), new Vector2(interRight, interTop)));

        if (interTop < box.TopRight.Y)
        {
            var boxRight = new Box2(new Vector2(box.BottomLeft.X, interTop), box.TopRight);
            if (!boxRight.IsEmpty())
                results.Add(boxRight);
        }

        if (interBottom > box.BottomLeft.Y)
        {
            var boxBottom = new Box2(box.BottomLeft, new Vector2(box.TopRight.X, interBottom));
            if (!boxBottom.IsEmpty())
                results.Add(boxBottom);
        }

        if (interLeft > box.BottomLeft.X)
        {
            var boxLeft = new Box2(new Vector2(box.BottomLeft.X, interBottom), new Vector2(interLeft, interTop));
            if (!boxLeft.IsEmpty())
                results.Add(boxLeft);
        }

        if (interRight < box.TopRight.X)
        {
            var boxRight = new Box2(new Vector2(interRight, interBottom), new Vector2(box.TopRight.X, interTop));
            if (!boxRight.IsEmpty())
                results.Add(boxRight);
        }

        return results.ToArray();
    }
}
