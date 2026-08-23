using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Cam = CoreEssentials.Camera.Camera;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

/// <summary>
/// Tests for the built-in CameraComponent (camera anchor) and the component LateUpdate hook.
/// </summary>
public class CameraComponentTests : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // Ensure no test leaks a camera into the global main-camera slot.
            Cam.SetMainCamera(null);
        }
        _disposed = true;
    }

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    // ===== Attach / detach lifecycle =====

    [Fact]
    public void OnAttach_RegistersCameraAsMainCamera()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());

        Assert.Same(component.Camera, Cam.MainCamera);
    }

    [Fact]
    public void OnDetach_ClearsMainCamera()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());
        Assert.NotNull(Cam.MainCamera);

        entity.RemoveComponent<CameraComponent>();

        Assert.Null(Cam.MainCamera);
    }

    // ===== LateUpdate anchoring =====

    [Fact]
    public void LateUpdate_SyncsCameraPositionFromOwner()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());

        entity.Position = new Vector2(123, 456);
        component.LateUpdate(new GameTime());

        Assert.Equal(new Vector2(123, 456), component.Camera.Position);
    }

    [Fact]
    public void LateUpdate_FollowsOwnerAsItMoves()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());

        // Simulate several frames where the owner moves (e.g. driven by WASD or physics).
        for (int i = 0; i < 5; i++)
        {
            entity.Position += new Vector2(10, -10);
            component.LateUpdate(new GameTime());
        }

        Assert.Equal(new Vector2(50, -50), component.Camera.Position);
    }

    [Fact]
    public void LateUpdate_SyncsRotationByDefault()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());

        entity.Rotation = 1.2f;
        component.LateUpdate(new GameTime());

        Assert.Equal(1.2f, component.Camera.Rotation);
    }

    [Fact]
    public void LateUpdate_SkipsRotationWhenSyncRotationDisabled()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());
        component.SyncRotation = false;

        entity.Rotation = 1.2f;
        component.LateUpdate(new GameTime());

        Assert.Equal(0f, component.Camera.Rotation);
    }

    [Fact]
    public void LateUpdate_PreservesZoomSetOnComponent()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());
        component.Zoom = 2.5f;

        entity.Position = new Vector2(10, 20);
        component.LateUpdate(new GameTime());

        Assert.Equal(2.5f, component.Camera.Zoom);
    }

    [Fact]
    public void LateUpdate_WithoutOwner_DoesNotThrow()
    {
        var component = new CameraComponent(); // never attached

        component.LateUpdate(new GameTime()); // should be a no-op

        Assert.Equal(Vector2.Zero, component.Camera.Position);
    }

    // ===== Component LateUpdate is driven by the entity lifecycle =====

    private class LateUpdateProbe : EntityComponent
    {
        public int LateUpdateCalls;
        public override void Update(GameTime gameTime) { }
        public override void LateUpdate(GameTime gameTime) => LateUpdateCalls++;
    }

    [Fact]
    public void EntityOnLateUpdate_DrivesComponentLateUpdate()
    {
        var entity = new TestEntity();
        var probe = entity.AddComponent(new LateUpdateProbe());

        entity.OnLateUpdate(new GameTime());
        entity.OnLateUpdate(new GameTime());

        Assert.Equal(2, probe.LateUpdateCalls);
    }

    [Fact]
    public void EntityOnLateUpdate_DoesNotDriveRegularUpdate()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CameraComponent());

        // Only OnLateUpdate is called (as the EntitySystem does after the update pass).
        entity.OnLateUpdate(new GameTime());

        Assert.Equal(Vector2.Zero, component.Camera.Position);
    }
}
