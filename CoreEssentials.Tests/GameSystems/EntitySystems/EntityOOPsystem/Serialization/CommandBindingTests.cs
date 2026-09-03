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

    // ──────────────────────────── Component discovery (Unity-style) ────────────────────────────

    [Fact]
    public void Discovery_ConcreteComponentBySimpleName_IsCreatedWithoutRegistration()
    {
        var factory = new DefaultComponentFactory();

        var component = factory.Create("DiscoveryFixtureComponent");

        Assert.IsType<DiscoveryFixtureComponent>(component);
    }

    [Fact]
    public void Discovery_ExplicitRegistrationBeatsDiscoveredType()
    {
        // A same-name registration must shadow whatever the assembly scan finds —
        // DiscoveryFixtureComponent is discoverable, but the registration wins.
        var factory = new DefaultComponentFactory();
        factory.Register("DiscoveryFixtureComponent", () => new DiscoveryShadowWinner());

        Assert.IsType<DiscoveryShadowWinner>(factory.Create("DiscoveryFixtureComponent"));
    }

    [Fact]
    public void Discovery_UnknownName_ReturnsNull()
    {
        var factory = new DefaultComponentFactory();

        Assert.Null(factory.Create("NoSuchComponentAnywhere"));
    }

    // ──────────────────────────── Helpers ────────────────────────────

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

/// <summary>Component whose handler throws — used to verify the bridge swallows errors.</summary>
public class ExplodingComponent : EntityComponent
{
    public event Action? Boom;

    public void OnBoom() => throw new InvalidOperationException("boom");

    /// <summary>Raises <see cref="Boom"/>.</summary>
    public void RaiseBoom() => Boom?.Invoke();
}

/// <summary>Discovered by the assembly scan — never explicitly registered anywhere.</summary>
public class DiscoveryFixtureComponent : EntityComponent
{
    public int Count;
    public void Increment() => Count++;
}

/// <summary>Returned by an explicit registration that shadows a same-named discoverable type.</summary>
public class DiscoveryShadowWinner : EntityComponent
{
}
