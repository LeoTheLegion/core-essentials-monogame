#nullable enable
using System;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization;

/// <summary>
/// Tests for declarative &lt;Bind&gt; event-to-command wiring.
/// The entity is the middleman: XML binds a public event on the entity or one of its
/// components to a public handler method resolved across the entity, its components and
/// its ancestors.
/// </summary>
public class CommandBindingTests : IDisposable
{
    private readonly EntitySystem _system = new();
    private readonly Game _mockGame;

    public CommandBindingTests()
    {
        // The component-factory regression tests construct real Canvas components, which
        // create Myra widgets — the GUI engine must be up.
        _mockGame = new Game1();
        GUIManager.Init(_mockGame, 800, 600);
    }

    // ──────────────────────────── Command form (SendMessage style) ────────────────────────────

    [Fact]
    public void Bind_CommandForm_ResolvesHandlerOnEntity_AndFires()
    {
        var entity = _system.CreateEntity<ClickableEntity>();
        var def = BuildDefinition(
            @"<Bind Event=""Signaled"" Command=""OnClicked"" />");

        // Attach a component that has the event, then apply the binds.
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        CommandBindings.ApplyBindings(entity, def);

        signal.RaiseSignal();

        Assert.Equal(1, ((ClickableEntity)entity).ClickCount);
    }

    [Fact]
    public void Bind_CommandForm_ResolvesHandlerOnComponent()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        var def = BuildDefinition(@"<Bind Event=""Signaled"" Command=""OnSignaled"" />");

        CommandBindings.ApplyBindings(entity, def);

        signal.RaiseSignal();

        Assert.Equal(1, signal.SignalCount);
    }

    [Fact]
    public void Bind_CommandForm_ResolvesHandlerOnAncestor()
    {
        var parent = _system.CreateEntity<ClickableEntity>();
        var child = new PlainEntity();
        parent.AddChild(child);

        var signal = (SignalComponent)child.AddComponent(new SignalComponent());
        var def = BuildDefinition(@"<Bind Event=""Signaled"" Command=""OnClicked"" />");

        CommandBindings.ApplyBindings(child, def);

        signal.RaiseSignal();

        Assert.Equal(1, ((ClickableEntity)parent).ClickCount);
    }

    // ──────────────────────────── Target+Member form (PersistentCall style) ────────────────────────────

    [Fact]
    public void Bind_TargetMemberForm_ResolvesNamedComponent()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        var def = BuildDefinition(@"<Bind Event=""Signaled"" Target=""SignalComponent"" Member=""OnSignaled"" />");

        CommandBindings.ApplyBindings(entity, def);

        signal.RaiseSignal();

        Assert.Equal(1, signal.SignalCount);
    }

    [Fact]
    public void Bind_TargetMemberForm_ResolvesEntityItself()
    {
        var entity = _system.CreateEntity<ClickableEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        var def = BuildDefinition(@"<Bind Event=""Signaled"" Target=""ClickableEntity"" Member=""OnClicked"" />");

        CommandBindings.ApplyBindings(entity, def);

        signal.RaiseSignal();

        Assert.Equal(1, ((ClickableEntity)entity).ClickCount);
    }

    [Fact]
    public void Bind_TargetMemberForm_RequiresBothAttributes()
    {
        var entity = _system.CreateEntity<ClickableEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());

        // Member without Target must not bind.
        CommandBindings.ApplyBindings(entity, BuildDefinition(@"<Bind Event=""Signaled"" Member=""OnClicked"" />"));
        signal.RaiseSignal();
        Assert.Equal(0, ((ClickableEntity)entity).ClickCount);
    }

    // ──────────────────────────── Payload delivery ────────────────────────────

    [Fact]
    public void Bind_EventHandlerEvent_DeliversPayloadToHandler()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        var payload = (PayloadComponent)entity.AddComponent(new PayloadComponent());
        var def = BuildDefinition(@"<Bind Event=""Ponged"" Command=""OnPonged"" />");

        CommandBindings.ApplyBindings(entity, def);

        payload.RaisePong(new EventArgs());

        Assert.Equal("System.EventArgs", payload.LastPayload);
    }

    [Fact]
    public void Bind_ZeroParamHandler_OnPayloadEvent_StillFires()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        var def = BuildDefinition(@"<Bind Event=""Pinged"" Command=""OnSignaled"" />");

        CommandBindings.ApplyBindings(entity, def);

        signal.RaisePing();

        Assert.Equal(1, signal.SignalCount);
    }

    // ──────────────────────────── Multi-bind and sources ────────────────────────────

    [Fact]
    public void Bind_MultipleBinds_AllFire()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        var def = BuildDefinition(@"
            <Bind Event=""Signaled"" Command=""OnSignaled"" />
            <Bind Event=""Pinged"" Command=""OnPinged"" />");

        CommandBindings.ApplyBindings(entity, def);

        signal.RaiseSignal();
        signal.RaisePing();

        Assert.Equal(1 + 10, signal.SignalCount);
    }

    [Fact]
    public void Bind_SourceAttribute_RestrictsEventLookup()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        var payload = (PayloadComponent)entity.AddComponent(new PayloadComponent());
        // Both components expose different events; Source pins the lookup to one.
        var def = BuildDefinition(@"<Bind Event=""Ponged"" Source=""PayloadComponent"" Command=""OnPonged"" />");

        CommandBindings.ApplyBindings(entity, def);

        payload.RaisePong(new EventArgs());

        Assert.Equal("System.EventArgs", payload.LastPayload);
    }

    // ──────────────────────────── Failure modes (warn, never throw) ────────────────────────────

    [Fact]
    public void Bind_UnknownCommand_DoesNotThrow_AndDoesNotBind()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        var signal = (SignalComponent)entity.AddComponent(new SignalComponent());
        var def = BuildDefinition(@"<Bind Event=""Signaled"" Command=""NoSuchCommand"" />");

        var ex = Record.Exception(() => CommandBindings.ApplyBindings(entity, def));

        Assert.Null(ex);
        signal.RaiseSignal(); // nothing subscribed
        Assert.Equal(0, signal.SignalCount);
    }

    [Fact]
    public void Bind_UnknownEvent_DoesNotThrow()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        entity.AddComponent(new SignalComponent());

        var ex = Record.Exception(() => CommandBindings.ApplyBindings(entity, BuildDefinition(@"<Bind Event=""NoSuchEvent"" Command=""OnSignaled"" />")));

        Assert.Null(ex);
    }

    [Fact]
    public void Bind_MissingEventAttribute_DoesNotThrow()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        entity.AddComponent(new SignalComponent());

        var ex = Record.Exception(() => CommandBindings.ApplyBindings(entity, BuildDefinition(@"<Bind Command=""OnSignaled"" />")));

        Assert.Null(ex);
    }

    [Fact]
    public void Bind_HandlerThrows_ExceptionIsSwallowed()
    {
        var entity = _system.CreateEntity<PlainEntity>();
        entity.AddComponent(new SignalComponent());
        var exploding = (ExplodingComponent)entity.AddComponent(new ExplodingComponent());
        // Command form: OnBoom is found on the ExplodingComponent.
        var def = BuildDefinition(@"<Bind Event=""Signaled"" Target=""ExplodingComponent"" Member=""OnBoom"" />");

        CommandBindings.ApplyBindings(entity, def);

        var ex = Record.Exception(() => exploding.RaiseBoom());

        Assert.Null(ex);
    }

    // ──────────────────────────── LoadSceneFromXml integration ────────────────────────────

    [Fact]
    public void LoadSceneFromXml_BindWiring_EndToEnd()
    {
        var xml = @"
            <Scene>
                <EntityDefinition Type=""ClickCounterEntity"" Id=""counter"">
                    <Components>
                        <Component Type=""SignalComponent"" />
                    </Components>
                    <Bind Event=""Signaled"" Command=""OnClicked"" />
                </EntityDefinition>
            </Scene>";

        var roots = EntitySerializer.LoadSceneFromXml(xml, _system, CreateTestFactory());

        Assert.Single(roots);
        var counter = (ClickCounterEntity)roots[0];
        var signal = (SignalComponent)counter.GetComponent<SignalComponent>()!;

        signal.RaiseSignal();

        Assert.Equal(1, counter.ClickCount);
    }

    [Fact]
    public void LoadSceneFromXml_BindInsideComponentsElement_IsApplied()
    {
        // Binds nested inside <Components> (sibling of <Component>) must be found too.
        var xml = @"
            <Scene>
                <EntityDefinition Type=""ClickCounterEntity"" Id=""counter"">
                    <Components>
                        <Component Type=""SignalComponent"" />
                        <Bind Event=""Signaled"" Command=""OnClicked"" />
                    </Components>
                </EntityDefinition>
            </Scene>";

        var roots = EntitySerializer.LoadSceneFromXml(xml, _system, CreateTestFactory());
        var counter = (ClickCounterEntity)roots[0];
        var signal = (SignalComponent)counter.GetComponent<SignalComponent>()!;

        signal.RaiseSignal();

        Assert.Equal(1, counter.ClickCount);
    }

    [Fact]
    public void LoadSceneFromXml_ReferenceOntoComponent_ResolvesTarget()
    {
        // A component-level <Reference> should resolve the entity by Id and set it on the component.
        var xml = @"
            <Scene>
                <EntityDefinition Type=""LabelLikeEntity"" Id=""label"">
                    <Position X=""0"" Y=""0"" />
                </EntityDefinition>
                <EntityDefinition Type=""KeeperEntity"" Id=""keeper"">
                    <Components>
                        <Component Type=""ScoreKeeperComponent"" />
                    </Components>
                    <References>
                        <Reference Name=""Target"" TargetId=""label"" />
                    </References>
                </EntityDefinition>
            </Scene>";

        var roots = EntitySerializer.LoadSceneFromXml(xml, _system, CreateTestFactory());

        var keeper = (KeeperEntity)_system.FindById("keeper")!;
        var label = (LabelLikeEntity)_system.FindById("label")!;
        var keeperComponent = (ScoreKeeperComponent)keeper.GetComponent<ScoreKeeperComponent>()!;

        Assert.Same(label, keeperComponent.Target);

        keeperComponent.Bump();
        Assert.Equal("Score: 1", label.LastText);
    }

    // ──────────────────────────── Component factory regression ────────────────────────────

    [Fact]
    public void DefaultComponentFactory_ConstructorPreRegistersBuiltIns()
    {
        // A bare factory must be able to create built-in components — passing a custom
        // factory to scene loading replaces the default, not augments it.
        var factory = new DefaultComponentFactory();

        Assert.NotNull(factory.Create("ButtonComponent"));
        Assert.NotNull(factory.Create("CanvasComponent"));
        Assert.NotNull(factory.Create("LabelComponent"));
        Assert.NotNull(factory.Create("AnchorComponent"));
    }

    [Fact]
    public void LoadScene_CustomFactoryWithBuiltInComponents_AttachesAll()
    {
        // Regression: a custom factory that only registers its own components used to
        // silently drop every built-in component from the scene (no canvas, no widgets).
        var xml = @"
            <Scene>
                <EntityDefinition Type=""CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization.PlainEntity"" Id=""root"">
                    <Components>
                        <Component Type=""CanvasComponent"" />
                        <Component Type=""ScoreKeeperComponent"" />
                    </Components>
                </EntityDefinition>
            </Scene>";

        var factory = new DefaultComponentFactory();
        factory.Register("ScoreKeeperComponent", () => new ScoreKeeperComponent());

        var roots = EntitySerializer.LoadSceneFromXml(xml, _system, factory);

        var root = (PlainEntity)roots.Single(r => r.Id == "root");
        Assert.NotNull(root.GetComponent<CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn.CanvasComponent>());
        Assert.NotNull(root.GetComponent<ScoreKeeperComponent>());
    }

    // ──────────────────────────── Helpers ────────────────────────────

    /// <summary>Factory registering the test fixture components by their short names.</summary>
    private static IComponentFactory CreateTestFactory()
    {
        var factory = new DefaultComponentFactory();
        factory.Register("SignalComponent", () => new SignalComponent());
        factory.Register("ScoreKeeperComponent", () => new ScoreKeeperComponent());
        return factory;
    }

    private static XElement BuildDefinition(string innerXml) =>
        XElement.Parse($"<EntityDefinition>{innerXml}</EntityDefinition>");

    public void Dispose()
    {
        _system.Dispose();
        EngineResolver.GetEngine().Shutdown();
        _mockGame.Dispose();
    }
}

// ──────────────────────────── Test fixtures (public so the assembly scan can resolve them by name) ────────────────────────────

/// <summary>Concrete entity with no behavior — a neutral host for components.</summary>
public class PlainEntity : Entity
{
}

/// <summary>Entity whose own public method is the command handler.</summary>
public class ClickableEntity : Entity
{
    public int ClickCount;
    public void OnClicked() => ClickCount++;
}

/// <summary>Component exposing an Action event and a public handler method.</summary>
public class SignalComponent : EntityComponent
{
    public event Action? Signaled;
    public event EventHandler? Pinged;
    public int SignalCount;

    public void OnSignaled() => SignalCount++;
    public void OnPinged(object? args) => SignalCount += 10;

    /// <summary>Raises <see cref="Signaled"/> from outside the class (events can't be invoked externally).</summary>
    public void RaiseSignal() => Signaled?.Invoke();

    /// <summary>Raises <see cref="Pinged"/> with an empty payload.</summary>
    public void RaisePing() => Pinged?.Invoke(this, EventArgs.Empty);
}

/// <summary>Component with a handler that takes the event payload (EventHandler style).</summary>
public class PayloadComponent : EntityComponent
{
    public event EventHandler? Ponged;
    public string? LastPayload;

    public void OnPonged(object? args) => LastPayload = args?.ToString();

    /// <summary>Raises <see cref="Ponged"/> with the given payload.</summary>
    public void RaisePong(EventArgs args) => Ponged?.Invoke(this, args);
}

/// <summary>Component holding an entity reference, settable from XML via &lt;Reference&gt;.</summary>
public class ScoreKeeperComponent : EntityComponent
{
    public Entity? Target;
    public int Value;

    public void Bump()
    {
        Value++;
        if (Target is LabelLikeEntity label)
            label.LastText = $"Score: {Value}";
    }
}

/// <summary>Entity mimicking a label that a command handler can update.</summary>
public class LabelLikeEntity : Entity
{
    public string? LastText;
}

/// <summary>Component whose handler throws — used to verify the bridge swallows errors.</summary>
public class ExplodingComponent : EntityComponent
{
    public event Action? Boom;

    public void OnBoom() => throw new InvalidOperationException("boom");

    /// <summary>Raises <see cref="Boom"/>.</summary>
    public void RaiseBoom() => Boom?.Invoke();
}

/// <summary>Scene-loadable entity with a public command handler (must be non-private for the assembly scan).</summary>
public class ClickCounterEntity : Entity
{
    public int ClickCount;
    public void OnClicked() => ClickCount++;
}

/// <summary>Scene-loadable entity hosting the ScoreKeeperComponent.</summary>
public class KeeperEntity : Entity
{
}
