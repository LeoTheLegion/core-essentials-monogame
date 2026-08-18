using System;

namespace CoreEssentials.Utils;

/// <summary>
/// A non-cryptographic random number generator utility for game logic.
/// Use this instead of <see cref="Random"/> for game-related randomness to make the intent clear
/// and avoid SonarQube warnings about cryptographic strength.
/// </summary>
public static class GameRandom
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    public static int Next() => _random.Next();

    /// <summary>
    /// Returns a non-negative random integer less than <paramref name="maxValue"/>.
    /// </summary>
    public static int Next(int maxValue) => _random.Next(maxValue);

    /// <summary>
    /// Returns a random integer in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).
    /// </summary>
    public static int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    /// <summary>
    /// Returns a random float between 0.0 and 1.0.
    /// </summary>
    public static float NextFloat() => (float)_random.NextDouble();

    /// <summary>
    /// Returns a random float in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).
    /// </summary>
    public static float NextFloat(float minValue, float maxValue) => minValue + NextFloat() * (maxValue - minValue);

    /// <summary>
    /// Returns a random boolean with 50% chance of being true.
    /// </summary>
    public static bool NextBool() => _random.Next(2) == 1;

    /// <summary>
    /// Returns a random boolean with the given probability of being true (0.0 to 1.0).
    /// </summary>
    public static bool NextBool(float probability) => NextFloat() < probability;

    /// <summary>
    /// Selects a random element from an array.
    /// </summary>
    public static T? Pick<T>(T[] items)
    {
        if (items == null || items.Length == 0) return default;
        return items[Next(items.Length)];
    }

    /// <summary>
    /// Returns a random direction vector with the given magnitude.
    /// </summary>
    public static Microsoft.Xna.Framework.Vector2 RandomDirection(float magnitude = 1f)
    {
        float angle = NextFloat() * MathF.PI * 2f;
        return new Microsoft.Xna.Framework.Vector2(
            MathF.Cos(angle),
            MathF.Sin(angle)) * magnitude;
    }

    /// <summary>
    /// Returns a random value in the range [-1, 1].
    /// </summary>
    public static float NextSignedFloat() => (NextFloat() * 2f) - 1f;

    /// <summary>
    /// Returns a random Vector2 with both components in the range [-1, 1].
    /// </summary>
    public static Microsoft.Xna.Framework.Vector2 RandomVector2() => new(NextSignedFloat(), NextSignedFloat());
}
