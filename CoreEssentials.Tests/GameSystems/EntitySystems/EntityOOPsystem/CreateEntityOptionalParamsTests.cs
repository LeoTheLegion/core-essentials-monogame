using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

/// <summary>
/// Tests for constructor resolution with optional parameters in EntitySystem.CreateEntity /
/// CreateEntityUnstarted (#69). Before the fix, Activator.CreateInstance required an exact
/// arity match and threw MissingMethodException when trailing optional args were omitted.
/// </summary>
public class CreateEntityOptionalParamsTests
{
    private readonly EntitySystem _system = new();

    // ===== Test entity types with various ctor shapes =====

    private class OptionalParamEntity : Entity
    {
        public Vector2 SpawnPosition { get; }
        public float Speed { get; }

        public OptionalParamEntity(Vector2 position, float speed = 1f)
        {
            SpawnPosition = position;
            Speed = speed;
        }

        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    private class MultiOptionalEntity : Entity
    {
        public string Name { get; }
        public float Scale { get; }
        public bool Radiation { get; }

        // Mirrors the real-world ShootingGallery.FloatingPopUpText 6-param case.
        public MultiOptionalEntity(Vector2 position, float duration, string name, Color color, float scale = 1f, bool radiation = false)
        {
            Name = name;
            Scale = scale;
            Radiation = radiation;
        }

        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    private class NoOptionalEntity : Entity
    {
        public int Value { get; }

        public NoOptionalEntity(int value)
        {
            Value = value;
        }

        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    private class ParamlessAndOptionalEntity : Entity
    {
        public bool UsedOptionalCtor { get; }

        public ParamlessAndOptionalEntity()
        {
            UsedOptionalCtor = false;
        }

        public ParamlessAndOptionalEntity(int seed)
        {
            UsedOptionalCtor = true;
        }

        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    // ===== CreateEntity with optional params (#69 repro) =====

    [Fact]
    public void CreateEntity_OmittingOptionalParam_UsesDefaultValue()
    {
        // Compiles fine as `new OptionalParamEntity(new Vector2(5, 5))`, but threw
        // MissingMethodException via Activator.CreateInstance before the fix.
        var entity = _system.CreateEntity<OptionalParamEntity>(new Vector2(5, 5));

        Assert.Equal(new Vector2(5, 5), entity.SpawnPosition);
        Assert.Equal(1f, entity.Speed);
    }

    [Fact]
    public void CreateEntity_PassingAllParams_UsesProvidedValues()
    {
        var entity = _system.CreateEntity<OptionalParamEntity>(new Vector2(0, 0), 3.5f);

        Assert.Equal(3.5f, entity.Speed);
    }

    [Fact]
    public void CreateEntity_TypeOverload_OmittingOptionalParam_Works()
    {
        var entity = _system.CreateEntity(typeof(OptionalParamEntity), new object[] { new Vector2(1, 2) });

        Assert.IsType<OptionalParamEntity>(entity);
        Assert.Equal(1f, ((OptionalParamEntity)entity).Speed);
    }

    [Fact]
    public void CreateEntity_MultipleOmittedOptionals_FillAllDefaults()
    {
        // 4 of 6 args: scale and radiation omitted.
        var entity = _system.CreateEntity<MultiOptionalEntity>(
            new Vector2(0, 0), 1.5f, "popup", Color.White);

        Assert.Equal("popup", entity.Name);
        Assert.Equal(1f, entity.Scale);
        Assert.False(entity.Radiation);
    }

    [Fact]
    public void CreateEntity_OneOmittedOptional_FillsOnlyThatDefault()
    {
        var entity = _system.CreateEntity<MultiOptionalEntity>(
            new Vector2(0, 0), 1.5f, "popup", Color.Red, 2f);

        Assert.Equal(2f, entity.Scale);
        Assert.False(entity.Radiation);
    }

    [Fact]
    public void CreateEntity_NoOptionalParams_StillWorks()
    {
        var entity = _system.CreateEntity<NoOptionalEntity>(42);

        Assert.Equal(42, entity.Value);
    }

    // ===== CreateEntityUnstarted shares the same resolution =====

    [Fact]
    public void CreateEntityUnstarted_OmittingOptionalParam_UsesDefaultValue()
    {
        var entity = _system.CreateEntityUnstarted(typeof(OptionalParamEntity), new object[] { new Vector2(7, 8) });

        Assert.Equal(new Vector2(7, 8), ((OptionalParamEntity)entity).SpawnPosition);
        Assert.Equal(1f, ((OptionalParamEntity)entity).Speed);
    }

    // ===== Error reporting =====

    [Fact]
    public void CreateEntity_TooFewRequiredArgs_ThrowsDescriptiveError()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => _system.CreateEntity(typeof(NoOptionalEntity), Array.Empty<object>()));

        // The new error lists available constructors instead of a bare MissingMethodException.
        Assert.Contains("No matching constructor", ex.Message);
        Assert.Contains(nameof(NoOptionalEntity), ex.Message);
        Assert.Contains("(Int32 value)", ex.Message);
    }

    [Fact]
    public void CreateEntity_TooManyArgs_ThrowsDescriptiveError()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => _system.CreateEntity(typeof(OptionalParamEntity), new object[] { new Vector2(0, 0), 1f, "extra" }));

        Assert.Contains("No matching constructor", ex.Message);
    }

    // ===== Overload preference =====

    [Fact]
    public void CreateEntity_PrefersExactArityOverFewerParams()
    {
        // No args: the parameterless ctor must win over (int seed).
        var entity = _system.CreateEntity<ParamlessAndOptionalEntity>();

        Assert.False(entity.UsedOptionalCtor);
    }

    [Fact]
    public void CreateEntity_WithArg_MatchesWiderCtor()
    {
        var entity = _system.CreateEntity<ParamlessAndOptionalEntity>(123);

        Assert.True(entity.UsedOptionalCtor);
    }
}
