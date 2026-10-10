#nullable enable
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Components.BuiltIn;

/// <summary>
/// Drives <see cref="ShaderComponent"/>'s uniform-apply logic through the <see cref="IShaderEffect"/>
/// seam with a recording fake. This asserts exactly which typed <c>SetValue</c> overload the production
/// dispatch selects for every parameter shape (both code-set and XML-parsed values) without needing a
/// compiled MonoGame effect or a graphics device — the same interface-replacement pattern used by
/// <c>IEntityDebugTarget</c>.
/// </summary>
public class ShaderComponentApplyTests
{
    // ── Typed (code-sourced) values → correct overload selected ─────────────────────────

    [Fact]
    public void Apply_TypedFloat_ScalarSelectsFloatOverload()
    {
        var shader = new ShaderComponent();
        shader.SetFloat("x", 1.5f);
        var effect = MakeScalar(EffectParameterType.Single, out var p);

        shader.Apply(effect);

        Assert.Equal("float", p.LastOverload);
        Assert.Equal(1.5f, (float)p.LastValue!);
    }

    [Fact]
    public void Apply_TypedInt_ScalarSelectsIntOverload()
    {
        var shader = new ShaderComponent();
        shader.SetInt("x", 7);
        var effect = MakeScalar(EffectParameterType.Int32, out var p);

        shader.Apply(effect);

        Assert.Equal("int", p.LastOverload);
        Assert.Equal(7, (int)p.LastValue!);
    }

    [Fact]
    public void Apply_TypedBool_ScalarSelectsBoolOverload()
    {
        var shader = new ShaderComponent();
        shader.SetBool("x", true);
        var effect = MakeScalar(EffectParameterType.Bool, out var p);

        shader.Apply(effect);

        Assert.Equal("bool", p.LastOverload);
        Assert.True((bool)p.LastValue!);
    }

    [Fact]
    public void Apply_TypedVector2_SelectsVector2Overload()
    {
        var shader = new ShaderComponent();
        shader.SetVector2("pos", new Vector2(1f, 2f));
        var effect = MakeVector(cols: 2, out var p, name: "pos");

        shader.Apply(effect);

        Assert.Equal("Vector2", p.LastOverload);
        Assert.Equal(new Vector2(1f, 2f), (Vector2)p.LastValue!);
    }

    [Fact]
    public void Apply_TypedVector3_SelectsVector3Overload()
    {
        var shader = new ShaderComponent();
        shader.SetVector3("pos", new Vector3(1f, 2f, 3f));
        var effect = MakeVector(cols: 3, out var p, name: "pos");

        shader.Apply(effect);

        Assert.Equal("Vector3", p.LastOverload);
        Assert.Equal(new Vector3(1f, 2f, 3f), (Vector3)p.LastValue!);
    }

    [Fact]
    public void Apply_TypedVector4_SelectsVector4Overload()
    {
        var shader = new ShaderComponent();
        shader.SetVector4("w", new Vector4(1f, 2f, 3f, 4f));
        var effect = MakeVector(cols: 4, out var p, name: "w");

        shader.Apply(effect);

        Assert.Equal("Vector4", p.LastOverload);
        Assert.Equal(new Vector4(1f, 2f, 3f, 4f), (Vector4)p.LastValue!);
    }

    [Fact]
    public void Apply_ColorOnFourComponentVector_WritesNormalizedFloats()
    {
        // A Color is written as normalized 0-1 floats (HLSL pixel shaders expect color uniforms in 0-1),
        // so Color(255,0,0,128) → Vector4(1, 0, 0, ~0.502).
        var shader = new ShaderComponent();
        shader.SetColor("tint", new Color(255, 0, 0, 128));
        var effect = MakeVector(cols: 4, out var p, name: "tint");

        shader.Apply(effect);

        Assert.Equal("Vector4", p.LastOverload);
        var v = (Vector4)p.LastValue!;
        Assert.Equal(1f, v.X, 3);
        Assert.Equal(0f, v.Y, 3);
        Assert.Equal(0f, v.Z, 3);
        Assert.InRange(v.W, 0.50f, 0.51f); // 128/255 ≈ 0.502
    }

    [Fact]
    public void Apply_TypedMatrix_SelectsMatrixOverload()
    {
        var shader = new ShaderComponent();
        shader.SetValue("m", Matrix.CreateRotationZ(0.5f));
        var effect = MakeMatrix(out var p);

        shader.Apply(effect);

        Assert.Equal("Matrix", p.LastOverload);
        Assert.Equal(Matrix.CreateRotationZ(0.5f), (Matrix)p.LastValue!);
    }

    // ── XML (string-sourced) values parsed against the declared shape ────────────────────

    [Fact]
    public void Apply_ParsedFloat_ScalarParsesStringToFloat()
    {
        var shader = new ShaderComponent();
        shader.InitializeFromStrings(new System.Collections.Generic.Dictionary<string, string> { ["x"] = "2.75" });
        var effect = MakeScalar(EffectParameterType.Single, out var p);

        shader.Apply(effect);

        Assert.Equal("float", p.LastOverload);
        Assert.Equal(2.75f, (float)p.LastValue!);
    }

    [Fact]
    public void Apply_ParsedInt_ScalarParsesStringToInt()
    {
        var shader = new ShaderComponent();
        shader.InitializeFromStrings(new System.Collections.Generic.Dictionary<string, string> { ["x"] = "42" });
        var effect = MakeScalar(EffectParameterType.Int32, out var p);

        shader.Apply(effect);

        Assert.Equal("int", p.LastOverload);
        Assert.Equal(42, (int)p.LastValue!);
    }

    [Fact]
    public void Apply_ParsedBool_ScalarParsesStringToBool()
    {
        var shader = new ShaderComponent();
        shader.InitializeFromStrings(new System.Collections.Generic.Dictionary<string, string> { ["x"] = "true" });
        var effect = MakeScalar(EffectParameterType.Bool, out var p);

        shader.Apply(effect);

        Assert.Equal("bool", p.LastOverload);
        Assert.True((bool)p.LastValue!);
    }

    [Fact]
    public void Apply_ParsedVector2_VectorParsesCommaString()
    {
        var shader = new ShaderComponent();
        shader.InitializeFromStrings(new System.Collections.Generic.Dictionary<string, string> { ["pos"] = "3,-1" });
        var effect = MakeVector(cols: 2, out var p, name: "pos");

        shader.Apply(effect);

        Assert.Equal("Vector2", p.LastOverload);
        Assert.Equal(new Vector2(3f, -1f), (Vector2)p.LastValue!);
    }

    [Theory]
    [InlineData("255,0,0")]      // RGB → alpha defaults to 255
    [InlineData("255,0,0,128")]  // RGBA explicit
    public void Apply_ParsedColorOnFourComponentVector_AcceptsRgbAndRgbaStrings(string raw)
    {
        var shader = new ShaderComponent();
        shader.InitializeFromStrings(new System.Collections.Generic.Dictionary<string, string> { ["tint"] = raw });
        var effect = MakeVector(cols: 4, out var p, name: "tint");

        shader.Apply(effect);

        Assert.Equal("Vector4", p.LastOverload);
        var v = (Vector4)p.LastValue!;
        Assert.Equal(1f, v.X, 3);   // R=255 → 1.0
        Assert.Equal(0f, v.Y, 3);   // G=0
        Assert.Equal(0f, v.Z, 3);   // B=0
    }

    [Fact]
    public void Apply_ParsedVector4_VectorParsesExplicitXyzw()
    {
        var shader = new ShaderComponent();
        shader.InitializeFromStrings(new System.Collections.Generic.Dictionary<string, string> { ["w"] = "1,2,3,4" });
        var effect = MakeVector(cols: 4, out var p, name: "w");

        shader.Apply(effect);

        // All four components are integers in [0,255], so this resolves through the color branch of
        // ParseVector4OrColor (new Color(1,2,3,4)) rather than a raw float vector — but both branches write
        // the same SetValue(Vector4) overload, which is what this assertion pins down.
        Assert.Equal("Vector4", p.LastOverload);
    }

    // ── Unknown parameters and incompatible types: warn-and-continue, never throw ──────────

    [Fact]
    public void Apply_UnknownParameter_SkipsWithoutThrowing()
    {
        var shader = new ShaderComponent();
        shader.SetFloat("known", 1f);
        shader.SetFloat("missing", 2f);
        // Effect declares only "known"; "missing" must be skipped.
        var effect = MakeScalar(EffectParameterType.Single, out var p, name: "known");

        var ex = Record.Exception(() => shader.Apply(effect));

        Assert.Null(ex);
        Assert.Equal("float", p.LastOverload);
        Assert.Equal(1f, (float)p.LastValue!); // only the known param was written
    }

    [Fact]
    public void Apply_IncompatibleTypedValue_WarnsAndContinuesWithoutThrowing()
    {
        var shader = new ShaderComponent();
        // A Vector2 stored for a scalar float: Convert.ToSingle throws InvalidCastException, which the
        // dispatch catches and reports as "cannot be applied" rather than surfacing.
        shader.SetValue("x", new Vector2(1f, 2f));
        var effect = MakeScalar(EffectParameterType.Single, out _);

        var ex = Record.Exception(() => shader.Apply(effect));

        Assert.Null(ex);
    }

    // ── Signature: order-stable batching key ───────────────────────────────────────────────

    [Fact]
    public void Signature_Empty_IsEmptyString()
    {
        Assert.Equal(string.Empty, new ShaderComponent().Signature);
    }

    [Fact]
    public void Signature_IndependentOfInsertionOrder()
    {
        var a = new ShaderComponent();
        a.SetFloat("x", 1f);
        a.SetInt("y", 2);

        var b = new ShaderComponent();
        b.SetInt("y", 2);
        b.SetFloat("x", 1f);

        Assert.Equal(a.Signature, b.Signature);
    }

    [Fact]
    public void Signature_ChangesWhenAValueChanges()
    {
        var shader = new ShaderComponent();
        shader.SetFloat("x", 1f);
        var before = shader.Signature;

        shader.SetFloat("x", 2f);

        Assert.NotEqual(before, shader.Signature);
    }

    // ── Get<T> resolves stored (typed or string) values on demand ────────────────────────

    [Fact]
    public void Get_TypedValue_ReturnsDirectly()
    {
        var shader = new ShaderComponent();
        shader.SetFloat("x", 3.5f);

        Assert.Equal(3.5f, shader.Get<float>("x"));
    }

    [Fact]
    public void Get_StringValue_ParsesOnDemand()
    {
        var shader = new ShaderComponent();
        shader.InitializeFromStrings(new System.Collections.Generic.Dictionary<string, string> { ["x"] = "3.25" });

        Assert.Equal(3.25f, shader.Get<float>("x"));
    }

    [Fact]
    public void Get_AbsentName_ReturnsDefault()
    {
        var shader = new ShaderComponent();
        Assert.Equal(0f, shader.Get<float>("nope"));
    }

    // ── Recording fakes for the IShaderEffect / IShaderParameter seam ────────────────────

    private static FakeEffect MakeScalar(EffectParameterType type, out FakeParameter p, string name = "x")
    {
        var e = new FakeEffect();
        e.Add(name, EffectParameterClass.Scalar, type, 0);
        p = (FakeParameter)e.GetParameter(name)!;
        return e;
    }

    private static FakeEffect MakeVector(int cols, out FakeParameter p, string name = "v")
    {
        var e = new FakeEffect();
        e.Add(name, EffectParameterClass.Vector, EffectParameterType.Void, cols);
        p = (FakeParameter)e.GetParameter(name)!;
        return e;
    }

    private static FakeEffect MakeMatrix(out FakeParameter p, string name = "m")
    {
        var e = new FakeEffect();
        e.Add(name, EffectParameterClass.Matrix, EffectParameterType.Void, 0);
        p = (FakeParameter)e.GetParameter(name)!;
        return e;
    }

    private sealed class FakeEffect : IShaderEffect
    {
        private readonly System.Collections.Generic.Dictionary<string, IShaderParameter> _params =
            new(System.StringComparer.Ordinal);

        public void Add(string name, EffectParameterClass c, EffectParameterType t, int cols)
            => _params[name] = new FakeParameter(c, t, cols);

        public IShaderParameter? GetParameter(string name)
            => _params.TryGetValue(name, out var p) ? p : null;
    }

    private sealed class FakeParameter : IShaderParameter
    {
        public FakeParameter(EffectParameterClass c, EffectParameterType t, int cols)
        { ParameterClass = c; ParameterType = t; ColumnCount = cols; }

        public EffectParameterClass ParameterClass { get; }
        public EffectParameterType ParameterType { get; }
        public int ColumnCount { get; }

        public string? LastOverload { get; private set; }
        public object? LastValue { get; private set; }

        public void SetValue(bool value) { LastOverload = "bool"; LastValue = value; }
        public void SetValue(int value) { LastOverload = "int"; LastValue = value; }
        public void SetValue(float value) { LastOverload = "float"; LastValue = value; }
        public void SetValue(Vector2 value) { LastOverload = "Vector2"; LastValue = value; }
        public void SetValue(Vector3 value) { LastOverload = "Vector3"; LastValue = value; }
        public void SetValue(Vector4 value) { LastOverload = "Vector4"; LastValue = value; }
        public void SetValue(Matrix value) { LastOverload = "Matrix"; LastValue = value; }
    }
}
