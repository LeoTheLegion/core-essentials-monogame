using Microsoft.Xna.Framework;

namespace CoreEssentials.Audio;

/// <summary>
/// Pure, allocation-free math for the opt-in 2D spatialization on <c>AudioSourceComponent</c>.
/// MonoGame has no native spatial audio, so these helpers derive a stereo pan and a distance
/// attenuation from each source's offset relative to the listener — deliberately simple (linear)
/// and kept static so they are trivially unit-testable without a live listener or engine.
/// </summary>
public static class SpatialAudioMath
{
    /// <summary>
    /// Computes a stereo pan (-1 = fully left, 0 = center, +1 = fully right) from the horizontal
    /// offset between a source and the listener, normalized by <paramref name="maxDistance"/>.
    /// A source to the right of the listener pans right; one to the left pans left.
    /// </summary>
    /// <param name="listenerX">The listener's world X position.</param>
    /// <param name="sourceX">The source's world X position.</param>
    /// <param name="maxDistance">The distance at which a source is fully panned to the edge (must be &gt; 0).</param>
    public static float ComputePan(float listenerX, float sourceX, float maxDistance)
    {
        if (maxDistance <= 0f) return 0f;
        var offset = sourceX - listenerX;
        return MathHelper.Clamp(offset / maxDistance, -1f, 1f);
    }

    /// <summary>
    /// Computes a linear distance attenuation in [0, 1]: full volume (1) at or inside
    /// <paramref name="minDistance"/>, falling to silence (0) at or beyond
    /// <paramref name="maxDistance"/>, and linearly interpolated between the two.
    /// </summary>
    /// <param name="distance">The distance between source and listener.</param>
    /// <param name="minDistance">Distance within which the source is heard at full volume (must be &gt;= 0).</param>
    /// <param name="maxDistance">Distance beyond which the source is silent (must be greater than minDistance).</param>
    public static float ComputeAttenuation(float distance, float minDistance, float maxDistance)
    {
        if (maxDistance <= minDistance)
            return distance <= minDistance ? 1f : 0f;

        if (distance <= minDistance) return 1f;
        if (distance >= maxDistance) return 0f;

        return (maxDistance - distance) / (maxDistance - minDistance);
    }

    /// <summary>
    /// Euclidean distance between two world positions.
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b) => Vector2.Distance(a, b);
}
