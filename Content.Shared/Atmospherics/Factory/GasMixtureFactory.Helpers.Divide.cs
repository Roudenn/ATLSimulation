using Content.Shared.Atmospherics.GasMixtures;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    /// <summary>
    /// Splits a <see cref="IGasMixture"/> into two.
    /// </summary>
    /// <param name="c">The <see cref="IGasMixture"/> to split. This will be modified to contain the remaining moles.</param>
    /// <param name="cSplit">A <see cref="IGasMixture"/> that will be modified to contain
    /// the specified fraction of the original container's moles and the same temperature.</param>
    /// <param name="fraction">The fraction of moles to move to the new container. Clamped between 0 and 1.</param>
    [PublicAPI]
    public void Split<T1, T2>(ref T1 c, ref T2 cSplit, float fraction = 0.5f)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        fraction = Math.Clamp(fraction, 0f, 1f);
        NumericsHelpers.Multiply(c.Moles, fraction, GasBuffer1);

        cSplit.Moles = GasBuffer1;
        cSplit.Temperature = c.Temperature;

        NumericsHelpers.Sub(c.Moles, GasBuffer1);
        ClearBuffer(ref GasBuffer1);
    }

    /// <summary>
    /// Divides a source <see cref="IGasMixture"/> into a specified number of equal parts.
    /// </summary>
    /// <param name="c">The input <see cref="IGasMixture"/> to split.</param>
    /// <param name="cFrac">A temporary working <see cref="IGasMixture"/> that the method will use to
    /// fill the target array with dupes.</param>
    /// <param name="dividedArray">An array of <see cref="IGasMixture"/>s equally split from the source <see cref="IGasMixture"/>.
    /// This will be written to. This must be the same length as num.</param>
    /// <param name="num">The number of <see cref="IGasMixture"/>s
    /// to split the source <see cref="IGasMixture"/> into.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to divide the source container by zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of the divided array does not match the specified number of divisions.</exception>
    [PublicAPI]
    public void Divide<T1, T2>(T1 c, ref T2 cFrac, T2[] dividedArray, uint num)
        where T1 : IGasMixture
        where T2 : struct, IGasMixture // if we allowed classes you'd just have an array reffing the same obj
    {
        ArgumentOutOfRangeException.ThrowIfZero(num);
        ArgumentOutOfRangeException.ThrowIfNotEqual(dividedArray.Length, (int)num);

        var fraction = 1f / num;
        Split(ref c, ref cFrac, fraction);

        for (var i = 0; i < num; i++)
        {
            dividedArray[i] = cFrac;
        }
    }

        /// <summary>
    /// Splits a <see cref="IGasMixture"/> into two.
    /// </summary>
    /// <param name="c">The <see cref="IGasMixture"/> to split. This will be modified to contain the remaining moles.</param>
    /// <param name="cSplit">A <see cref="IGasMixture"/> that will be modified to contain
    /// the specified fraction of the original container's moles and the same temperature.</param>
    /// <param name="fraction">The fraction of moles to move to the new container. Clamped between 0 and 1.</param>
    [PublicAPI]
    public void SplitVelocity(ref VelocityGasMixture c, ref VelocityGasMixture cSplit, float fraction = 0.5f)
    {
        Split(ref c, ref cSplit);
        cSplit.Velocity = c.Velocity;
    }

    /// <summary>
    /// Divides a source <see cref="IGasMixture"/> into a specified number of equal parts.
    /// </summary>
    /// <param name="c">The input <see cref="IGasMixture"/> to split.</param>
    /// <param name="cFrac">A temporary working <see cref="IGasMixture"/> that the method will use to
    /// fill the target array with dupes.</param>
    /// <param name="dividedArray">An array of <see cref="IGasMixture"/>s equally split from the source <see cref="IGasMixture"/>.
    /// This will be written to. This must be the same length as num.</param>
    /// <param name="num">The number of <see cref="IGasMixture"/>s
    /// to split the source <see cref="IGasMixture"/> into.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to divide the source container by zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of the divided array does not match the specified number of divisions.</exception>
    [PublicAPI]
    public void DivideVelocity(VelocityGasMixture c, ref VelocityGasMixture cFrac, VelocityGasMixture[] dividedArray, uint num)
    {
        ArgumentOutOfRangeException.ThrowIfZero(num);
        ArgumentOutOfRangeException.ThrowIfNotEqual(dividedArray.Length, (int)num);

        var fraction = 1f / num;
        SplitVelocity(ref c, ref cFrac, fraction);

        for (var i = 0; i < num; i++)
        {
            dividedArray[i] = cFrac;
        }
    }
}
