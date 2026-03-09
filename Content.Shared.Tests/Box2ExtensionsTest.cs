using System.Numerics;
using Content.Shared.Maths;
using NUnit.Framework;
using Robust.Shared.Maths;

namespace Content.Shared.Tests;

[Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
[TestFixture, TestOf(typeof(Box2Extensions))]
public sealed class Box2ExtensionsTest
{
    [Test]
    public void TestSplit()
    {
        var box1 = Box2.CenteredAround(Vector2.Zero, new Vector2(4f));

        var boxTopRight = new Box2(new Vector2(0f), new Vector2(5f));
        var boxBottomLeft = new Box2(new Vector2(-5f), new Vector2(0f));
        var boxTopLeft = new Box2(new Vector2(-5f, 0f), new Vector2(0f, 5f));
        var boxBottomRight = new Box2(new Vector2(0f, -5f), new Vector2(5f, 0f));

        var split1 = boxTopRight.AABBIntersection(box1);
        var split2 = boxBottomLeft.AABBIntersection(box1);
        var split3 = boxTopLeft.AABBIntersection(box1);
        var split4 = boxBottomRight.AABBIntersection(box1);
        string fail = string.Empty;
        foreach (var box in split1)
        {
            fail += box.ToString();
        }
        fail += "\n";
        foreach (var box in split2)
        {
            fail += box.ToString();
        }
        fail += "\n";
        foreach (var box in split3)
        {
            fail += box.ToString();
        }
        fail += "\n";
        foreach (var box in split4)
        {
            fail += box.ToString();
        }

        var union1 = new Box2();
        var union2 = new Box2();
        foreach (var box in split1)
        {
            union1 = union1.Union(box);
        }
        foreach (var box in split2)
        {
            union2 = union2.Union(box);
        }
        Assert.That(union1, Is.EqualTo(boxTopRight));
        Assert.That(union2, Is.EqualTo(boxBottomLeft));
        Assert.Pass(fail); // mission failed successfully, because i had named the varible in a funny way
    }
}
