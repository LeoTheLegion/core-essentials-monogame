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
    /// Parses a string in "X,Y" format into a Vector2.
    /// </summary>
    public static Vector2 ParseVector2FromString(string value)
    {
        var parts = value.Split(',');
        if (parts.Length >= 2 &&
            float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
            float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
        {
            return new Vector2(x, y);
        }
        return Vector2.Zero;
    }

    /// <summary>
    /// Parses a string into a Color, supporting named colors and fallback to White.
    /// </summary>
    public static Color ParseColor(string value)
    {
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
        return Color.White;
    }
}
