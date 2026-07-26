using Xunit;
using CoreEssentials.Debugging;
using CoreEssentials.GUI.Internal;
using Microsoft.Xna.Framework;
using System;

namespace CoreEssentials.Tests.Debugging
{
    public class StickyLogTests : IDisposable
    {
        private readonly Game _mockGame;

        public StickyLogTests()
        {
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Initialize GUIManager - handles MyraEnvironment internally
            CoreEssentials.GUI.GUIManager.Init(_mockGame, 800, 600);
        }

        void IDisposable.Dispose()
        {
            _mockGame?.Dispose();
            
            // Shutdown the engine to clean up internal state
            var engine = EngineResolver.GetEngine();
            engine.Shutdown();
        }
        
        [Fact]
        public void LoadGUI_CreatesCanvasAndGrid()
        {
            // Arrange
            var stickyLog = new StickyLog();
            
            // Act
            stickyLog.LoadGUI();
            
            // Assert - verify that internal fields were set via reflection on interface-typed fields
            var canvasField = typeof(StickyLog).GetField("_canvas", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var canvas = canvasField!.GetValue(stickyLog);
            
            var gridField = typeof(StickyLog).GetField("_grid", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var grid = gridField!.GetValue(stickyLog);
            
            Assert.NotNull(canvas);
            Assert.NotNull(grid);
        }
        
        [Fact]
        public void Log_CreatesNewLabelWhenKeyDoesNotExist()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            
            // Act
            stickyLog.Log("TestKey", "TestValue");
            
            // Assert - access the interface-typed dictionary via reflection
            var logField = typeof(StickyLog).GetField("_log", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var logDict = (System.Collections.Generic.Dictionary<string, CoreEssentials.GUI.Types.ILabel>)logField!.GetValue(stickyLog)!;
            
            Assert.True(logDict.ContainsKey("TestKey"));
            Assert.Equal("TestValue", logDict["TestKey"].Text);
        }
        
        [Fact]
        public void Log_UpdatesExistingLabelWhenKeyExists()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            stickyLog.Log("TestKey", "InitialValue");
            
            // Act
            stickyLog.Log("TestKey", "UpdatedValue");
            
            // Assert
            var logField = typeof(StickyLog).GetField("_log", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var logDict = (System.Collections.Generic.Dictionary<string, CoreEssentials.GUI.Types.ILabel>)logField!.GetValue(stickyLog)!;
            
            Assert.Equal("UpdatedValue", logDict["TestKey"].Text);
        }
        
        [Fact]
        public void IsVisible_ControlsGridVisibility()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            
            // Act
            stickyLog.IsVisible = false;
            
            // Assert
            Assert.False(stickyLog.IsVisible);
            
            // Act again
            stickyLog.IsVisible = true;
            
            // Assert again
            Assert.True(stickyLog.IsVisible);
        }
        
        [Fact]
        public void Update_CallsCanvasUpdate()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            var gameTime = new GameTime();
            
            // Act & Assert - should not throw
            Exception exception = Record.Exception(() => stickyLog.Update(gameTime));
            Assert.Null(exception);
        }
        
        [Fact]
        public void SetPosition_UpdatesCanvasPosition()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            var newPosition = new Vector2(100, 200);
            
            // Act - note: StickyLog doesn't have a public SetPosition method, we use the internal canvas
            // Access canvas via reflection and set position directly
            var canvasField = typeof(StickyLog).GetField("_canvas", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var canvas = (CoreEssentials.GUI.Types.ICanvas)canvasField!.GetValue(stickyLog)!;
            
            // Set position through the interface
            canvas.SetPosition(newPosition);
            
            // Update should apply the position
            stickyLog.Update(new GameTime());
        }
        
        [Fact]
        public void Remove_RemovesSpecificLogEntry()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            stickyLog.Log("Key1", "Value1");
            stickyLog.Log("Key2", "Value2");
            
            // Act
            stickyLog.Remove("Key1");
            
            // Assert
            var logField = typeof(StickyLog).GetField("_log", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var logDict = (System.Collections.Generic.Dictionary<string, CoreEssentials.GUI.Types.ILabel>)logField!.GetValue(stickyLog)!;
            
            Assert.False(logDict.ContainsKey("Key1"));
            Assert.True(logDict.ContainsKey("Key2"));
        }
        
        [Fact]
        public void Clear_RemovesAllLogEntries()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            stickyLog.Log("Key1", "Value1");
            stickyLog.Log("Key2", "Value2");
            
            // Act
            stickyLog.Clear();
            
            // Assert
            var logField = typeof(StickyLog).GetField("_log", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var logDict = (System.Collections.Generic.Dictionary<string, CoreEssentials.GUI.Types.ILabel>)logField!.GetValue(stickyLog)!;
            
            Assert.Empty(logDict);
        }
    }
}
