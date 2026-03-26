using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Utils;

public sealed class RobustArrayPool<T>
{
    private readonly T[][] _buffer;
    public int Length { get; private set; }

    private readonly Func<T>? _factory;

    public RobustArrayPool(int arraySize, int maxBucketSize, Func<T>? factory = null, bool init = false)
    {
        _buffer = new T[maxBucketSize][];
        _factory = factory;
        Length = maxBucketSize - 1;

        for (int i = 0; i < maxBucketSize; i++)
        {
            _buffer[i] = new T[arraySize];

            if(!init || factory == null)
                continue;

            for (int j = 0; j < arraySize; j++)
            {
                _buffer[j][i] = factory();
            }
        }
    }

    public T[] Rent()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(Length);
        var objectSelected = _buffer[Length];
        Length--;
        return objectSelected;
    }

    public bool TryRent([NotNullWhen(true)] out T[]? obj)
    {
        obj = null;
        if (Length < 0)
            return false;

        var objectSelected = _buffer[Length];
        Length--;
        obj = objectSelected;
        return true;
    }

    public void Return(T[] obj, bool clearArray = false)
    {
        if (clearArray)
        {
            var objSpan = obj.AsSpan();
            objSpan.Clear();
            if (_factory != null)
                objSpan.Fill(_factory());
            objSpan.CopyTo(obj);
        }

        Length++;
        _buffer[Length] = obj;
    }

    public void Clear()
    {
        Length = 0;
    }
}
