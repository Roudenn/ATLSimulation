using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Utility;

namespace Content.Shared.Utils;

/// <summary>
/// A version of <see cref="ArrayPool{T}"/> that pools a fixed amount of arrays with a fixed size.
/// Useful for cases where you need to easily manage large amounts of buffers to store objects.
/// </summary>
/// <remarks>
/// This is different from the <see cref="ArrayPool{T}"/>
/// </remarks>
/// <typeparam name="T"></typeparam>
public sealed class RobustArrayPool<T>
{
    private readonly T[][] _buffer;

    /// <summary>
    /// Initializer for elements of the arrays that this pool stores.
    /// </summary>
    private readonly Func<T>? _factory;

    /// <summary>
    /// Amount of remaining arrays in the pool.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Size of the pooled arrays.
    /// </summary>
    public int ArraySize { get; private set; }

    public RobustArrayPool(int arraySize, int maxBucketSize, Func<T>? factory = null, bool init = false)
    {
        _buffer = new T[maxBucketSize][];
        _factory = factory;
        Length = maxBucketSize - 1;
        ArraySize = arraySize;

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

    /// <summary>
    /// Takes an array from the pool.
    /// </summary>
    /// <returns>An array from the pool.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the array pool no longer has available arrays.</exception>
    public T[] Rent()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(Length);
        var objectSelected = _buffer[Length];
        Length--;
        return objectSelected;
    }

    /// <summary>
    /// Tries to get an array from the pool.
    /// </summary>
    /// <param name="obj">An available array from this pool.</param>
    /// <returns>True if the array was found.</returns>
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

    /// <summary>
    /// Returns an array back to the pool.
    /// </summary>
    /// <remarks>
    /// The array has to
    /// </remarks>
    /// <param name="obj"></param>
    /// <param name="clearArray"></param>
    public void Return(T[] obj, bool clearArray = false)
    {
        DebugTools.AssertEqual(obj.Length, ArraySize);

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
}
