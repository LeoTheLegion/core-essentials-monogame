using System;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Internal;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GUI;

/// <summary>
/// Tests the canvas registration lifecycle: a canvas must only join the global GUI once its owning
/// scene actually starts pumping it (first update/draw), and leave when cleaned up. Registering at
/// construction made canvases render for scenes that were not (or no longer) active — e.g. a target
/// scene's GUI showing through while it loads, and a loading screen persisting after the swap.
/// </summary>
public class CanvasRegistrationLifecycleTests : IDisposable
{
    private readonly Game _mockGame;
    private bool _disposed;

    public CanvasRegistrationLifecycleTests()
    {
        _mockGame = new Game1();
        GUIManager.Init(_mockGame, 800, 600);
    }

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
            _mockGame?.Dispose();
            EngineResolver.GetEngine().Shutdown();
        }
        _disposed = true;
    }

    /// <summary>
    /// Reads the canvas implementation's internal registration flag — the contract under test: a canvas
    /// is not registered in the global GUI until its first pump, and is unregistered on cleanup. The
    /// factory returns the wrapper, so unwrap to the concrete impl that owns the flag.
    /// </summary>
    private static bool IsRegistered(CoreEssentials.GUI.Types.ICanvas canvas)
    {
        var impl = (object)canvas;
        if (impl is not CoreEssentials.GUI.Engines.Myra.CanvasImpl canvasImpl)
            throw new InvalidOperationException("Expected a CanvasImpl-backed canvas.");

        var field = typeof(CoreEssentials.GUI.Engines.Myra.CanvasImpl)
            .GetField("_isRegistered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)field!.GetValue(canvasImpl)!;
    }

    [Fact]
    public void NewCanvas_IsNotInGlobalRoot_UntilFirstUpdate()
    {
        // Arrange — a canvas built exactly as a scene component would build one.
        var canvas = CanvasFactory.CreateScreenSpace();

        // Act/Assert — before it is pumped, it must NOT be registered in the global GUI root.
        Assert.False(IsRegistered(canvas), "Canvas should not register until its first pump.");

        // Pumping it (as a live scene's component update would) attaches it to the global GUI.
        canvas.Update(new GameTime());
        Assert.True(IsRegistered(canvas), "First pump should register the canvas in the global GUI.");
    }

    [Fact]
    public void CleanUp_RemovesCanvasFromGlobalRoot_AfterItWasPumped()
    {
        // Arrange — build and pump so the canvas is registered.
        var canvas = CanvasFactory.CreateScreenSpace();
        canvas.Update(new GameTime());
        Assert.True(IsRegistered(canvas));

        // Act — cleaning up (as a scene unload would) detaches it from the global GUI.
        canvas.CleanUp();

        // Assert
        Assert.False(IsRegistered(canvas), "CleanUp should unregister the canvas from the global GUI.");
    }

    [Fact]
    public void CleanUp_BeforeAnyUpdate_DoesNotThrow()
    {
        // A canvas cleaned up before it was ever pumped must not throw (it was never registered).
        var canvas = CanvasFactory.CreateScreenSpace();

        Assert.Null(Record.Exception(() => canvas.CleanUp()));
        Assert.False(IsRegistered(canvas));
    }
}
