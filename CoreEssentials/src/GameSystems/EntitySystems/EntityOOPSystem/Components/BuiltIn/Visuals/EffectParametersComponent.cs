using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Owns and controls the shader uniforms ("vars") for the effect on its entity's sprite.
/// Attach this alongside a <see cref="SpriteComponent"/> that declares an effect: it holds the
/// named uniform values and the render pipeline pushes them onto the effect immediately before each
/// sprite batch's <c>SpriteBatch.Begin</c>.
/// </summary>
/// <remarks>
/// Values can come from two sources:
/// <list type="bullet">
/// <item><b>Code</b> — the typed setters (<see cref="SetFloat"/>, <see cref="SetColor"/>, …) store a
/// ready-typed value that is written straight onto the matching effect parameter.</item>
/// <item><b>Data-driven XML</b> — <c>&lt;EffectParameter Name="X" Value="Y"/&gt;</c> arrives as raw
/// strings via <see cref="InitializeFromStrings"/>. The target type is resolved from the effect's own
/// parameter list at apply time, so XML needs no type hints.</item>
/// </list>
/// A code-set value for a name always wins over an XML-set value for the same name (last write wins by
/// call order). <see cref="Signature"/> is a canonical, order-stable representation of the current
/// values; the render pipeline uses it as part of its batching key so sprites sharing an effect and the
/// same var values batch together while differing values split into separate runs.
/// </remarks>
public class EffectParametersComponent : EntityComponent
{
    private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);
    private readonly HashSet<string> _warnedUnknown = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the current uniform values (name → value). XML-sourced values appear as their raw string;
    /// code-sourced values appear as their typed object.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Values => _values;

    /// <summary>
    /// Gets a canonical, order-stable signature of the current values. Two components with the same set
    /// of (name → value) pairs produce the same signature regardless of insertion order; any change to a
    /// value changes the signature. Used as the render pipeline's per-var batching key.
    /// </summary>
    public string Signature
    {
        get
        {
            if (_values.Count == 0) return string.Empty;

            var names = new List<string>(_values.Keys);
            names.Sort(StringComparer.Ordinal);

            var parts = new List<string>(names.Count);
            foreach (var name in names)
                parts.Add(name + "=" + Convert.ToString(_values[name], CultureInfo.InvariantCulture));

            return string.Join(";", parts);
        }
    }

    // ──────────────────────────── Typed setters (code path) ────────────────────────────

    /// <summary>Sets a float uniform by name.</summary>
    public void SetFloat(string name, float value) => _values[name] = value;

    /// <summary>Sets an int uniform by name.</summary>
    public void SetInt(string name, int value) => _values[name] = value;

    /// <summary>Sets a bool uniform by name.</summary>
    public void SetBool(string name, bool value) => _values[name] = value;

    /// <summary>Sets a color uniform by name (applied to a float4 parameter).</summary>
    public void SetColor(string name, Color value) => _values[name] = value;

    /// <summary>Sets a Vector2 uniform by name.</summary>
    public void SetVector2(string name, Vector2 value) => _values[name] = value;

    /// <summary>Sets a Vector3 uniform by name.</summary>
    public void SetVector3(string name, Vector3 value) => _values[name] = value;

    /// <summary>Sets a Vector4 uniform by name.</summary>
    public void SetVector4(string name, Vector4 value) => _values[name] = value;

    /// <summary>
    /// Sets a uniform of any supported type by name. The boxed value is written onto the matching effect
    /// parameter at apply time, so it must already be the correct CLR type (float, int, bool, Color,
    /// Vector2/3/4).
    /// </summary>
    public void SetValue(string name, object? value) => _values[name] = value;

    // ──────────────────────────── Typed getters ────────────────────────────

    /// <summary>
    /// Gets the current value of a uniform as type <typeparamref name="T"/>, or null when the name is
    /// absent. An XML-sourced (raw string) value is parsed to <typeparamref name="T"/> on demand using
    /// the shared serialization parser.
    /// </summary>
    public T? Get<T>(string name)
    {
        if (!_values.TryGetValue(name, out var stored))
            return default;

        if (stored is T typed)
            return typed;

        if (stored is string raw)
        {
            try
            {
                return (T)SerializationUtils.ParseValue(typeof(T), raw);
            }
            catch
            {
                return default;
            }
        }

        return default;
    }

    // ──────────────────────────── XML path ────────────────────────────

    /// <summary>
    /// Seeds this component with raw string values parsed from a data-driven scene. Values are stored as
    /// strings and resolved against the effect parameter's type at apply time, so no type hints are needed
    /// in XML. A later typed setter for the same name overrides the seeded value.
    /// </summary>
    public void InitializeFromStrings(IReadOnlyDictionary<string, string>? values)
    {
        if (values == null) return;

        foreach (var (name, raw) in values)
            _values[name] = raw;
    }

    // ──────────────────────────── Apply to an effect ────────────────────────────

    /// <summary>
    /// Writes every stored uniform onto the matching parameter of <paramref name="effect"/>. Code-sourced
    /// values are set directly; XML-sourced (raw string) values are parsed against the parameter's own
    /// declared type first so the boxed value MonoGame receives is correctly typed. Names the effect does
    /// not declare, or values that cannot be applied to the parameter's type, are skipped with a one-time
    /// console warning — a typo never throws mid-render.
    /// </summary>
    public void ApplyTo(Effect? effect)
    {
        if (effect == null || _values.Count == 0) return;

        foreach (var (name, stored) in _values)
        {
            var parameter = effect.Parameters[name];
            if (parameter == null)
            {
                WarnOnce(name, () => $"Effect has no parameter '{name}' — skipped. " +
                    $"Current: {string.Join(", ", _values.Keys)}.");
                continue;
            }

            try
            {
                var applied = stored is string raw
                    ? SetParsedParameter(parameter, raw)
                    : SetTypedParameter(parameter, stored!);

                if (!applied)
                    WarnOnce(name + "::type", () => $"'{name}' (stored as " +
                        $"{(stored == null ? "null" : stored.GetType().Name)}) cannot be applied to a " +
                        $"{parameter.ParameterClass}/{parameter.ParameterType} parameter.");
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or NotSupportedException or OverflowException)
            {
                WarnOnce(name + "::parse", () => $"Could not apply '{name}' to a " +
                    $"{parameter.ParameterClass}/{parameter.ParameterType} parameter: {ex.Message}");
            }
        }
    }

    /// <summary>Removes all stored uniforms.</summary>
    public void Clear() => _values.Clear();

    private void WarnOnce(string key, Func<string> message)
    {
        if (_warnedUnknown.Add(key))
            Console.WriteLine($"[EffectParameters] " + message());
    }

    /// <summary>
    /// Applies a code-sourced (already-typed) value to a parameter using the overload matching its
    /// declared shape. Numeric scalars are coerced; a <see cref="Color"/> is applied to a 4-component
    /// vector. Returns false when the stored CLR type is incompatible with the parameter's shape.
    /// </summary>
    private static bool SetTypedParameter(EffectParameter parameter, object value)
    {
        try
        {
            switch (parameter.ParameterClass)
            {
                case EffectParameterClass.Scalar:
                    if (parameter.ParameterType == EffectParameterType.Bool)
                        parameter.SetValue(Convert.ToBoolean(value));
                    else if (parameter.ParameterType == EffectParameterType.Int32)
                        parameter.SetValue(Convert.ToInt32(value));
                    else
                        parameter.SetValue(Convert.ToSingle(value));
                    return true;

                case EffectParameterClass.Vector:
                    switch (parameter.ColumnCount)
                    {
                        case 1:
                            parameter.SetValue(Convert.ToSingle(value));
                            return true;
                        case 2:
                            parameter.SetValue((Vector2)value);
                            return true;
                        case 3:
                            parameter.SetValue((Vector3)value);
                            return true;
                        default:
                            if (value is Color color)
                                parameter.SetValue(new Vector4(color.R, color.G, color.B, color.A));
                            else
                                parameter.SetValue((Vector4)value);
                            return true;
                    }

                case EffectParameterClass.Matrix:
                    parameter.SetValue((Matrix)value);
                    return true;

                default:
                    return false;
            }
        }
        catch (InvalidCastException)
        {
            return false;
        }
    }

    /// <summary>
    /// Parses a raw string against the parameter's declared shape and applies it. A 4-component vector
    /// accepts either an explicit "x,y,z,w" or an "R,G,B"/"R,G,B,A" color (glow tints are commonly
    /// authored as colors but live in a <c>float4</c>). Returns false for shapes with no string form.
    /// </summary>
    private static bool SetParsedParameter(EffectParameter parameter, string raw)
    {
        switch (parameter.ParameterClass)
        {
            case EffectParameterClass.Scalar:
                if (parameter.ParameterType == EffectParameterType.Bool)
                    parameter.SetValue(bool.Parse(raw));
                else if (parameter.ParameterType == EffectParameterType.Int32)
                    parameter.SetValue(int.Parse(raw, CultureInfo.InvariantCulture));
                else
                    parameter.SetValue(float.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture));
                return true;

            case EffectParameterClass.Vector:
                switch (parameter.ColumnCount)
                {
                    case 1:
                        parameter.SetValue(float.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture));
                        return true;
                    case 2:
                        parameter.SetValue(SerializationUtils.ParseVector2FromString(raw));
                        return true;
                    case 3:
                        parameter.SetValue(SerializationUtils.ParseVector3FromString(raw));
                        return true;
                    default:
                        parameter.SetValue(ParseVector4OrColor(raw));
                        return true;
                }

            // Matrices and object/struct parameters have no string form.
            default:
                return false;
        }
    }

    private static Vector4 ParseVector4OrColor(string raw)
    {
        var parts = raw.Split(',');
        if (parts.Length is 3 or 4 &&
            int.TryParse(parts[0], out int r) &&
            int.TryParse(parts[1], out int g) &&
            int.TryParse(parts[2], out int b))
        {
            int a = parts.Length == 4 && int.TryParse(parts[3], out var parsedA) ? parsedA : 255;
            if (r is >= 0 and <= 255 && g is >= 0 and <= 255 && b is >= 0 and <= 255 && a is >= 0 and <= 255)
            {
                var c = new Color(r, g, b, a);
                return new Vector4(c.R, c.G, c.B, c.A);
            }
        }

        return SerializationUtils.ParseVector4FromString(raw);
    }
}
