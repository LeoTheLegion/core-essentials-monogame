using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// The single Unity-style owner of an entity's shader: the <see cref="Effect"/> (and its asset) *and*
/// the effect's uniforms ("vars"). Attach this alongside a <see cref="SpriteComponent"/> — the sprite
/// renderer draws the quad and leans on this component for the shader; to change a var you get this
/// component and set it.
/// </summary>
/// <remarks>
/// <para><b>Effect.</b> An explicit <see cref="Effect"/> always wins over a declarative
/// <see cref="EffectAsset"/> (which is resolved through the <see cref="AssetManager"/> once in
/// <see cref="OnAttach"/>). When neither is set, <see cref="EffectiveEffect"/> is null and the entity
/// renders in the SpriteBatch's default batch — so a "basic" shader is simply one with no effect.</para>
/// <para><b>Vars.</b> Values can come from two sources: code (the typed setters store a ready-typed value
/// written straight onto the matching parameter) and data-driven XML (<c>&lt;EffectParameter Name="X"
/// Value="Y"/&gt;</c> arrives as raw strings via <see cref="InitializeFromStrings"/> and is resolved against
/// the effect's own parameter type at apply time, so XML needs no type hints). A code-set value for a name
/// always wins over an XML-set value by call order.</para>
/// <para><see cref="Signature"/> is a canonical, order-stable representation of the current values; the render
/// pipeline uses it as part of its batching key so sprites sharing an effect and the same var values batch
/// together while differing values split into separate runs. Values are pushed onto the effect immediately
/// before each sprite batch's <c>SpriteBatch.Begin</c>.</para>
/// </remarks>
public class ShaderComponent : EntityComponent
{
    // ──────────────────────────── Effect (shader reference) ────────────────────────────

    /// <summary>
    /// Gets or sets an explicit MonoGame <see cref="Effect"/> to render the sprite with. When set, it takes
    /// precedence over <see cref="EffectAsset"/> (same precedence as an explicit <c>Sprite</c> over a
    /// declarative asset). In MonoGame an effect is applied at <c>SpriteBatch.Begin</c>, so the render
    /// pipeline groups entities by effect and opens a dedicated Begin/End for each distinct effect.
    /// </summary>
    public Effect? Effect { get; set; }

    /// <summary>
    /// Gets or sets the asset name of an <see cref="Effect"/> to load via the <see cref="AssetManager"/>
    /// (e.g. "Effects/glow.xml"). Resolved once in <see cref="OnAttach"/> and assigned to
    /// <see cref="Effect"/> — but only when no explicit <see cref="Effect"/> was set already, so an effect
    /// assigned in code always wins. This lets data-driven (XML) entities declare a shader with a plain
    /// string property instead of needing a per-game loader component to bridge the gap.
    /// </summary>
    public string EffectAsset { get; set; } = "";

    /// <summary>
    /// Gets the effective <see cref="Effect"/> for this shader: the explicit <see cref="Effect"/> when set,
    /// otherwise the effect resolved from <see cref="EffectAsset"/>. Returns null when neither is set, in
    /// which case the sprite renders with the SpriteBatch's default (no shader) — preserving current behavior
    /// and batching exactly.
    /// </summary>
    public Effect? EffectiveEffect => Effect ?? _resolvedEffect;

    private Effect? _resolvedEffect;

    // ──────────────────────────── Vars (uniforms) ────────────────────────────

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

    // ──────────────────────────── Lifecycle ────────────────────────────

    /// <summary>
    /// Resolves <see cref="EffectAsset"/> (if set) through the <see cref="AssetManager"/> and exposes it via
    /// <see cref="EffectiveEffect"/> — unless an explicit <see cref="Effect"/> was already assigned in code.
    /// Runs once on attach, which is the earliest point at which XML-declared properties are final. Failures
    /// are logged and swallowed so a missing asset never breaks entity attachment.
    /// </summary>
    public override void OnAttach()
    {
        base.OnAttach();

        if (!string.IsNullOrWhiteSpace(EffectAsset) && Effect == null)
        {
            try
            {
                _resolvedEffect = AssetManager.LoadAsset<EffectAsset>(EffectAsset).Effect;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ShaderComponent] Could not load effect asset '{EffectAsset}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clears the resolved <see cref="EffectAsset"/> so a re-attached component can resolve it again. An
    /// explicitly assigned <see cref="Effect"/> is left untouched (it is code-owned).
    /// </summary>
    public override void OnDetach()
    {
        base.OnDetach();
        _resolvedEffect = null;
    }

    // ──────────────────────────── Private helpers ────────────────────────────

    private void WarnOnce(string key, Func<string> message)
    {
        if (_warnedUnknown.Add(key))
            Console.WriteLine($"[ShaderComponent] " + message());
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
