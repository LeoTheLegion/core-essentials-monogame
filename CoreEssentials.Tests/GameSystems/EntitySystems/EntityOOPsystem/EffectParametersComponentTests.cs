#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Tests for the built-in <see cref="EffectParametersComponent"/>: its typed setters, the
    /// order-stable <see cref="EffectParametersComponent.Signature"/>, on-demand typed getters over XML
    /// (raw-string) values, and the XML seeding path. Real MonoGame <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>
    /// parameters cannot be created without a live GraphicsDevice, so the actual write-onto-parameter
    /// dispatch in <c>ApplyTo</c> is exercised by the playground smoke-run; here we verify the pure-C#
    /// value-storage contract that feeds it.
    /// </summary>
    public class EffectParametersComponentTests
    {
        private sealed class TestEntity : Entity
        {
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
        }

        // ===== Typed setters store values =====

        [Fact]
        public void SetFloat_StoresValue()
        {
            var comp = new EffectParametersComponent();
            comp.SetFloat("GlowStrength", 1.5f);

            Assert.Equal(1.5f, comp.Get<float>("GlowStrength"));
            Assert.Contains("GlowStrength", comp.Values.Keys);
        }

        [Fact]
        public void SetInt_StoresValue()
        {
            var comp = new EffectParametersComponent();
            comp.SetInt("Steps", 42);

            Assert.Equal(42, comp.Get<int>("Steps"));
        }

        [Fact]
        public void SetBool_StoresValue()
        {
            var comp = new EffectParametersComponent();
            comp.SetBool("Enabled", true);

            Assert.True(comp.Get<bool>("Enabled"));
        }

        [Fact]
        public void SetColor_StoresValue()
        {
            var comp = new EffectParametersComponent();
            comp.SetColor("Tint", Color.Coral);

            Assert.Equal(Color.Coral, comp.Get<Color>("Tint"));
        }

        [Fact]
        public void SetVector2_3_4_StoreValues()
        {
            var comp = new EffectParametersComponent();
            comp.SetVector2("Offset", new Vector2(1f, 2f));
            comp.SetVector3("Normal", new Vector3(0f, 1f, 0f));
            comp.SetVector4("Raw", new Vector4(1f, 2f, 3f, 4f));

            Assert.Equal(new Vector2(1f, 2f), comp.Get<Vector2>("Offset"));
            Assert.Equal(new Vector3(0f, 1f, 0f), comp.Get<Vector3>("Normal"));
            Assert.Equal(new Vector4(1f, 2f, 3f, 4f), comp.Get<Vector4>("Raw"));
        }

        [Fact]
        public void SetValue_BoxedObject_StoresValue()
        {
            var comp = new EffectParametersComponent();
            comp.SetValue("Strength", 0.75f);

            Assert.Equal(0.75f, comp.Get<float>("Strength"));
        }

        // ===== Get<T> over raw (XML) strings =====

        [Fact]
        public void Get_TypedFromRawString_ParsesOnDemand()
        {
            var comp = new EffectParametersComponent();
            comp.InitializeFromStrings(new Dictionary<string, string> { ["Strength"] = "0.75" });

            // Stored as a raw string, but readable as a float on demand.
            Assert.Equal(0.75f, comp.Get<float>("Strength"));
        }

        [Fact]
        public void Get_VectorFromRawString_ParsesOnDemand()
        {
            var comp = new EffectParametersComponent();
            comp.InitializeFromStrings(new Dictionary<string, string> { ["Offset"] = "1.5,2.5" });

            Assert.Equal(new Vector2(1.5f, 2.5f), comp.Get<Vector2>("Offset"));
        }

        [Fact]
        public void Get_MissingName_ReturnsDefault()
        {
            var comp = new EffectParametersComponent();

            Assert.Equal(0f, comp.Get<float>("Absent"));
            Assert.Equal(default(Vector2), comp.Get<Vector2>("Absent"));
        }

        [Fact]
        public void CodeSetterOverridesXmlSeededValueForSameName()
        {
            var comp = new EffectParametersComponent();
            comp.InitializeFromStrings(new Dictionary<string, string> { ["Strength"] = "0.25" });
            comp.SetFloat("Strength", 0.9f);

            // Last write wins by call order: the code-set float replaces the raw string.
            Assert.Equal(0.9f, comp.Get<float>("Strength"));
        }

        // ===== Signature =====

        [Fact]
        public void Signature_Empty_ReturnsEmptyString()
        {
            var comp = new EffectParametersComponent();
            Assert.Equal(string.Empty, comp.Signature);
        }

        [Fact]
        public void Signature_IsOrderStable()
        {
            var a = new EffectParametersComponent();
            a.SetFloat("Alpha", 1f);
            a.SetFloat("Beta", 2f);

            var b = new EffectParametersComponent();
            b.SetFloat("Beta", 2f);
            b.SetFloat("Alpha", 1f);

            // Same (name → value) pairs, different insertion order → same signature.
            Assert.Equal(a.Signature, b.Signature);
        }

        [Fact]
        public void Signature_ChangesWhenAValueChanges()
        {
            var comp = new EffectParametersComponent();
            comp.SetFloat("Strength", 1f);
            var before = comp.Signature;

            comp.SetFloat("Strength", 2f);

            Assert.NotEqual(before, comp.Signature);
        }

        [Fact]
        public void Signature_DiffersWhenANamedValueDiffers()
        {
            var a = new EffectParametersComponent();
            a.SetFloat("Strength", 1f);

            var b = new EffectParametersComponent();
            b.SetFloat("Strength", 2f);

            Assert.NotEqual(a.Signature, b.Signature);
        }

        [Fact]
        public void Signature_DiffersWhenANameIsAdded()
        {
            var a = new EffectParametersComponent();
            a.SetFloat("Strength", 1f);

            var b = new EffectParametersComponent();
            b.SetFloat("Strength", 1f);
            b.SetBool("Enabled", true);

            Assert.NotEqual(a.Signature, b.Signature);
        }

        // ===== Clear =====

        [Fact]
        public void Clear_RemovesAllValues()
        {
            var comp = new EffectParametersComponent();
            comp.SetFloat("Strength", 1f);
            comp.SetBool("Enabled", true);

            comp.Clear();

            Assert.Empty(comp.Values);
            Assert.Equal(string.Empty, comp.Signature);
        }

        // ===== InitializeFromStrings (XML path) =====

        [Fact]
        public void InitializeFromNull_IsNoOp()
        {
            var comp = new EffectParametersComponent();
            comp.InitializeFromStrings(null);
            Assert.Empty(comp.Values);
        }

        [Fact]
        public void InitializeFromStrings_StoresRawValues()
        {
            var comp = new EffectParametersComponent();
            comp.InitializeFromStrings(new Dictionary<string, string>
            {
                ["Strength"] = "0.5",
                ["Tint"] = "255,140,30"
            });

            Assert.Equal(2, comp.Values.Count);
            // Raw strings are preserved verbatim until applied against a real parameter.
            Assert.Equal("0.5", comp.Values["Strength"]);
            Assert.Equal("255,140,30", comp.Values["Tint"]);
        }

        // ===== Entity.GetRenderEffectParameters =====

        [Fact]
        public void GetRenderEffectParameters_WithComponent_ReturnsIt()
        {
            var entity = new TestEntity();
            var comp = new EffectParametersComponent();
            entity.AddComponent(comp);

            Assert.Same(comp, entity.GetRenderEffectParameters());
        }

        [Fact]
        public void GetRenderEffectParameters_WithoutComponent_ReturnsNull()
        {
            var entity = new TestEntity();
            Assert.Null(entity.GetRenderEffectParameters());
        }
    }
}
