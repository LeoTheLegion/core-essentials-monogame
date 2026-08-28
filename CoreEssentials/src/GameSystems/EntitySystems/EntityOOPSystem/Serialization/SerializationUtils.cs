using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Shared utility for parsing strings into various types used during entity and component serialization.
/// </summary>
public static class SerializationUtils
{
    /// <summary>
    /// Parses a string value into the specified target type.
    /// Supports basic types, Vector2, Color, and Enums.
    /// </summary>
    /// <param name="targetType">The type to parse the value into.</param>
    /// <param name="valueString">The string representation of the value.</param>
    /// <returns>The parsed object of type targetType.</returns>
    /// <exception cref="NotSupportedException">Thrown if the type is not supported for parsing.</exception>
    public static object ParseValue(Type targetType, string valueString)
    {
        if (targetType == typeof(int))
            return int.Parse(valueString);
        if (targetType == typeof(float))
            return float.Parse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture);
        if (targetType == typeof(bool))
            return bool.Parse(valueString);
        if (targetType == typeof(string))
            return valueString;
        if (targetType == typeof(Vector2))
            return ParseVector2FromString(valueString);
        if (targetType == typeof(Color))
            return ParseColor(valueString);
        if (targetType.IsEnum)
            return Enum.Parse(targetType, valueString, ignoreCase: true);

        throw new NotSupportedException($"Parsing for type {targetType.Name} is not supported in SerializationUtils.");
    }

    /// <summary>
    /// Parses a string into a Vector2.
    /// Accepts "X,Y" format or a bare scalar, which is expanded to (v, v) for uniform scaling.
    /// </summary>
    public static Vector2 ParseVector2FromString(string value)
    {
        var parts = value.Split(',');
        if (parts.Length == 1 &&
            float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float scalar))
        {
            return new Vector2(scalar, scalar);
        }

        if (parts.Length >= 2 &&
            float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
            float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
        {
            return new Vector2(x, y);
        }

        Console.WriteLine($"[Serialization] Could not parse Vector2 from '{value}' — using (0, 0).");
        return Vector2.Zero;
    }

    /// <summary>
    /// Parses a string into a Color.
    /// Supports named colors (e.g. "LightGreen"), numeric "R,G,B" and "R,G,B,A" strings
    /// (e.g. "100,255,100"), with fallback to White for unrecognized input.
    /// </summary>
    public static Color ParseColor(string value)
    {
        // 1. Try numeric "R,G,B[,A]" format first — named colors never contain commas.
        var parts = value.Split(',');
        if ((parts.Length == 3 || parts.Length == 4) &&
            int.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out int r) &&
            int.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int g) &&
            int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int b))
        {
            int a = parts.Length == 4 && int.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out int alpha) ? alpha : 255;
            return new Color(
                (byte)Math.Clamp(r, 0, 255),
                (byte)Math.Clamp(g, 0, 255),
                (byte)Math.Clamp(b, 0, 255),
                (byte)Math.Clamp(a, 0, 255));
        }

        // 2. Try named colors from the Color static palette.
        try
        {
            var colorType = typeof(Color);
            var field = colorType.GetField(value, BindingFlags.Static | BindingFlags.Public);
            if (field != null)
                return (Color)field.GetValue(null)!;

            var prop = colorType.GetProperty(value, BindingFlags.Static | BindingFlags.Public);
            if (prop != null)
                return (Color)prop.GetValue(null)!;
        }
        catch
        {
            // Fall through to default
        }

        Console.WriteLine($"[Serialization] Could not parse Color from '{value}' — using White.");
        return Color.White;
    }
}
