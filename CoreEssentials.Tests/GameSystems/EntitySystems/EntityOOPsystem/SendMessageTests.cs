#nullable enable
using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

/// <summary>
/// Tests for scene-wide SendMessage (Unity-style multi-cast) and the public
/// entity-to-system access that it relies on.
/// </summary>
public class SendMessageTests : IDisposable
{
    private readonly EntitySystem _system = new();

    // ──────────────────────────── Core broadcast ────────────────────────────

    [Fact]
    public void SendMessage_InvokesHandlerOnEntity()
    {
        var entity = _system.CreateEntity<ReceiverEntity>();

        var invoked = _system.SendMessage("OnDamaged");

        Assert.Equal(1, invoked);
        Assert.Equal(1, entity.DamageCount);
    }

    [Fact]
    public void SendMessage_InvokesHandlerOnComponent()
    {
        var entity = _system.CreateEntity<PlainHostEntity>();
        var receiver = (ReceiverComponent)entity.AddComponent(new ReceiverComponent());

        var invoked = _system.SendMessage("OnDamaged");

        Assert.Equal(1, invoked);
        Assert.Equal(1, receiver.DamageCount);
    }

    [Fact]
    public void SendMessage_IsSceneWide_CrossSubtree()
    {
        // The key capability the old ancestor-walk <Bind> resolution lacks:
        // reactors in completely unrelated subtrees all receive the message.
        var rootA = _system.CreateEntity<PlainHostEntity>();
        var childA = new PlainHostEntity();
        rootA.AddChild(childA);
        var receiverA = (ReceiverComponent)childA.AddComponent(new ReceiverComponent());

        var rootB = _system.CreateEntity<PlainHostEntity>();
        var receiverB = (ReceiverComponent)rootB.AddComponent(new ReceiverComponent());

        var invoked = _system.SendMessage("OnDamaged");

        // Multi-cast: every matching handler in the scene fires, not just the first.
        Assert.Equal(2, invoked);
        Assert.Equal(1, receiverA.DamageCount);
        Assert.Equal(1, receiverB.DamageCount);
    }

    [Fact]
    public void SendMessage_DeliversPayloadToSingleParamHandler()
    {
        var entity = _system.CreateEntity<PlainHostEntity>();
        var receiver = (ReceiverComponent)entity.AddComponent(new ReceiverComponent());

        _system.SendMessage("OnHealed", 42);

        Assert.Equal(0, receiver.DamageCount);
        Assert.Equal(42, receiver.LastAmount);
    }

    [Fact]
    public void SendMessage_UnknownMessage_ReturnsZero()
    {
        _system.CreateEntity<PlainHostEntity>();

        Assert.Equal(0, _system.SendMessage("NoSuchMessage"));
    }

    [Fact]
    public void SendMessage_EmptyOrWhitespace_ReturnsZero()
    {
        Assert.Equal(0, _system.SendMessage(""));
        Assert.Equal(0, _system.SendMessage("   "));
    }

    // ──────────────────────────── Failure isolation ────────────────────────────

    [Fact]
    public void SendMessage_HandlerThrows_OtherHandlersStillRun()
    {
        var exploding = _system.CreateEntity<ExplodingReceiverEntity>();
        var healthy = (ReceiverComponent)_system.CreateEntity<PlainHostEntity>().AddComponent(new ReceiverComponent());

        var invoked = _system.SendMessage("OnDamaged");

        // The throwing handler is swallowed; the other one still fires.
        Assert.Equal(1, invoked);
        Assert.Equal(1, healthy.DamageCount);
    }

    [Fact]
    public void SendMessage_SnapshotIteration_HandlerSpawningEntity_DoesNotLoop()
    {
        var spawner = _system.CreateEntity<SpawningReceiverEntity>();
        spawner.SpawnInto = e => _system.CreateEntity<PlainHostEntity>();

        // A handler that spawns a new entity during the broadcast must not be
        // visited (the iteration runs over a snapshot) and must not throw.
        var ex = Record.Exception(() => _system.SendMessage("OnDamaged"));

        Assert.Null(ex);
        Assert.Equal(1, spawner.DamageCount);
    }

    // ──────────────────────────── Convenience surface (#79/#80) ────────────────────────────

    [Fact]
    public void Entity_SendMessage_ForwardsToSystem()
    {
        var entity = _system.CreateEntity<ReceiverEntity>();

        Assert.Equal(1, entity.SendMessage("OnDamaged"));
    }

    [Fact]
    public void Entity_SendMessage_OutsideSystem_ReturnsMinusOne()
    {
        var detached = new ReceiverEntity();

        Assert.Equal(-1, detached.SendMessage("OnDamaged"));
    }

    [Fact]
    public void Component_SendMessage_ForwardsToSystem()
    {
        var entity = _system.CreateEntity<ReceiverEntity>();
        var component = (PingComponent)entity.AddComponent(new PingComponent());

        Assert.Equal(1, component.SendMessage("OnDamaged"));
    }

    [Fact]
    public void GetEntitySystem_IsPublic_AndNullBeforeAdd()
    {
        var detached = new PlainHostEntity();
        Assert.Null(detached.GetEntitySystem());

        var attached = _system.CreateEntity<PlainHostEntity>();
        Assert.Same(_system, attached.GetEntitySystem());
    }

    [Fact]
    public void Component_EntitySystemAndGame_Properties_Resolve()
    {
        var entity = _system.CreateEntity<PlainHostEntity>();
        var component = (PingComponent)entity.AddComponent(new PingComponent());

        Assert.Same(_system, component.EntitySystem);

        // Game requires a live scene chain; outside one it must be null, not throw.
        Assert.Null(component.Game);
    }

    [Fact]
    public void Component_EntitySystem_NullBeforeOwnerIsInSystem()
    {
        var detached = new PingComponent();

        Assert.Null(detached.EntitySystem);
        Assert.Equal(-1, detached.SendMessage("OnDamaged"));
    }

    // ──────────────────────────── Unity-style create/destroy one-liners ────────────────────────────

    [Fact]
    public void Entity_CreateGameObject_SpawnsInSameSystem()
    {
        var parent = _system.CreateEntity<PlainHostEntity>();

        var spawned = parent.CreateGameObject<ReceiverEntity>();

        Assert.NotNull(spawned);
        Assert.Same(_system, spawned!.GetEntitySystem());
    }

    [Fact]
    public void Component_CreateGameObject_SpawnsInOwnersSystem()
    {
        var host = _system.CreateEntity<PlainHostEntity>();
        var spawner = (PingComponent)host.AddComponent(new PingComponent());

        var spawned = spawner.CreateGameObject<ReceiverEntity>();

        Assert.NotNull(spawned);
        Assert.Same(_system, spawned!.GetEntitySystem());
    }

    [Fact]
    public void CreateGameObject_Detached_ReturnsNull()
    {
        var detachedHost = new PlainHostEntity();
        var detachedComponent = (PingComponent)detachedHost.AddComponent(new PingComponent());

        Assert.Null(detachedHost.CreateGameObject<ReceiverEntity>());
        Assert.Null(detachedComponent.CreateGameObject<ReceiverEntity>());
    }

    [Fact]
    public void Component_DestroyOwner_MarksOwnerDestroyed()
    {
        var host = _system.CreateEntity<PlainHostEntity>();
        var component = (PingComponent)host.AddComponent(new PingComponent());

        component.DestroyOwner();

        Assert.True(host.Destroyed);
    }

    // ──────────────────────────── Prefab-style template instantiation ────────────────────────────

    [Fact]
    public void Entity_InstantiatePrefab_SpawnsPrefabInSameSystem()
    {
        RegisterHostPrefab("host");
        var parent = _system.CreateEntity<PlainHostEntity>();

        var prefab = parent.InstantiatePrefab("host", new Vector2(10, 20));

        Assert.NotNull(prefab);
        Assert.Same(_system, prefab!.GetEntitySystem());
    }

    [Fact]
    public void Component_InstantiatePrefab_SpawnsPrefabInOwnersSystem()
    {
        RegisterHostPrefab("host");
        var host = _system.CreateEntity<PlainHostEntity>();
        var spawner = (PingComponent)host.AddComponent(new PingComponent());

        var prefab = spawner.InstantiatePrefab("host", Vector2.Zero);

        Assert.NotNull(prefab);
        Assert.Same(_system, prefab!.GetEntitySystem());
    }

    [Fact]
    public void InstantiatePrefab_Detached_ReturnsNull()
    {
        RegisterHostPrefab("host");
        var detachedHost = new PlainHostEntity();

        Assert.Null(detachedHost.InstantiatePrefab("host", Vector2.Zero));
    }

    [Fact]
    public void Obsolete_InstantiateTemplate_ShimStillWorks()
    {
#pragma warning disable CS0618 // Intentionally exercising the obsolete shim
        RegisterHostPrefab("host");
        var parent = _system.CreateEntity<PlainHostEntity>();
        var prefab = parent.InstantiateTemplate("host", Vector2.Zero);
#pragma warning restore CS0618

        Assert.NotNull(prefab);
    }

    // ──────────────────────────── Binds on template instantiation ────────────────────────────

    [Fact]
    public void Template_BindElement_WiresCommandOnInstantiation()
    {
        _system.RegisterPrefab("signaler", CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization.EntityTemplateLoader.LoadFromXml(
            @"<EntityTemplate Type=""SignalingEntity"">
                <Bind Event=""Signaled"" Command=""OnSignalReceived"" />
            </EntityTemplate>"));

        var entity = (SignalingEntity)_system.Instantiate("signaler", Vector2.Zero);

        entity.Raise();

        Assert.Equal(1, entity.ReceivedCount);
    }

    [Fact]
    public void Template_BindElement_WiresEachInstantiationIndependently()
    {
        _system.RegisterPrefab("signaler", CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization.EntityTemplateLoader.LoadFromXml(
            @"<EntityTemplate Type=""SignalingEntity"">
                <Components>
                    <Bind Event=""Signaled"" Command=""OnSignalReceived"" />
                </Components>
            </EntityTemplate>"));

        var first = (SignalingEntity)_system.Instantiate("signaler", Vector2.Zero);
        var second = (SignalingEntity)_system.Instantiate("signaler", Vector2.Zero);

        first.Raise();

        Assert.Equal(1, first.ReceivedCount);
        Assert.Equal(0, second.ReceivedCount);
    }

    private void RegisterHostPrefab(string name) =>
        _system.RegisterPrefab(name, new CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization.Prefab
        {
            Type = nameof(PlainHostEntity)
        });

    public void Dispose() => _system.Dispose();
}

// ──────────────────────────── Test fixtures ────────────────────────────

/// <summary>Neutral entity host with no handlers of its own.</summary>
public class PlainHostEntity : Entity
{
}

/// <summary>Entity whose own public method is a message handler.</summary>
public class ReceiverEntity : Entity
{
    public int DamageCount;
    public void OnDamaged() => DamageCount++;
}

/// <summary>Component with zero- and one-parameter handlers for the same message.</summary>
public class ReceiverComponent : EntityComponent
{
    public int DamageCount;
    public int LastAmount;

    public void OnDamaged() => DamageCount++;
    public void OnHealed(int amount) => LastAmount = amount;
}

/// <summary>Minimal component used to exercise the convenience accessors.</summary>
public class PingComponent : EntityComponent
{
}

/// <summary>Entity whose handler throws — verifies broadcast isolation.</summary>
public class ExplodingReceiverEntity : Entity
{
    public void OnDamaged() => throw new InvalidOperationException("boom");
}

/// <summary>Entity whose handler spawns a new entity during the broadcast.</summary>
public class SpawningReceiverEntity : Entity
{
    public int DamageCount;
    public Action<Entity>? SpawnInto;

    public void OnDamaged()
    {
        DamageCount++;
        SpawnInto?.Invoke(this);
    }
}

/// <summary>Entity with its own event and handler, used to verify template binds.</summary>
public class SignalingEntity : Entity
{
    public int ReceivedCount;

    public event Action? Signaled;

    public void Raise() => Signaled?.Invoke();

    public void OnSignalReceived() => ReceivedCount++;
}
