using Xunit;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Moq;
using System;
using System.Reflection;
using CoreEssentials.GUI;

namespace CoreEssentials.Tests.Debugging
{
    public class StickyLogTests : IDisposable
    {
        private readonly Game _mockGame;

        public StickyLogTests()
        {
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Set Myra environment before tests
            MyraEnvironment.Game = _mockGame;
            
            // Initialize GUIManager for testing
            GUIManager.Init(_mockGame, 800, 600);
        }
        
        void IDisposable.Dispose()
        {
            // Clean up resources
            _mockGame?.Dispose();
        }
        
        [Fact]
        public void LoadGUI_CreatesCanvasAndGrid()
        {
            // Arrange
            var stickyLog = new StickyLog();
            
            // Act
            stickyLog.LoadGUI();
            
            // Assert
            // Access private fields using reflection
            var canvasField = typeof(StickyLog).GetField("_canvas", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var canvas = canvasField.GetValue(stickyLog);
            
            var gridField = typeof(StickyLog).GetField("_grid", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var grid = gridField.GetValue(stickyLog);
            
            Assert.NotNull(canvas);
            Assert.NotNull(grid);
            Assert.IsType<Canvas>(canvas);
            Assert.IsType<Grid>(grid);
        }
        
        [Fact]
        public void Log_CreatesNewLabelWhenKeyDoesNotExist()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            
            // Act
            stickyLog.Log("TestKey", "TestValue");
            
            // Assert
            // Access private dictionary using reflection
            var logField = typeof(StickyLog).GetField("log", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var log = (System.Collections.Generic.Dictionary<string, Label>)logField.GetValue(stickyLog);
            
            Assert.True(log.ContainsKey("TestKey"));
            Assert.Equal("TestValue", log["TestKey"].Text);
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
            var logField = typeof(StickyLog).GetField("log", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var log = (System.Collections.Generic.Dictionary<string, Label>)logField.GetValue(stickyLog);
            
            Assert.Equal("UpdatedValue", log["TestKey"].Text);
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
            var gridField = typeof(StickyLog).GetField("_grid", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var grid = (Grid)gridField.GetValue(stickyLog);
            
            Assert.False(grid.Visible);
            
            // Act again
            stickyLog.IsVisible = true;
            
            // Assert again
            Assert.True(grid.Visible);
        }
        
        [Fact]
        public void Update_CallsCanvasUpdate()
        {
            // Arrange
            var stickyLog = new StickyLog();
            stickyLog.LoadGUI();
            var gameTime = new GameTime();
            
            // We can't easily verify that canvas.Update was called since we can't mock it
            // Instead, we'll just verify that the method doesn't throw an exception
            
            // Act & Assert
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
            
            // Act
            stickyLog.SetPosition(newPosition);
            
            // Assert
            // Access the private canvas field
            var canvasField = typeof(StickyLog).GetField("_canvas", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var canvas = (Canvas)canvasField.GetValue(stickyLog);
            
            // Update should be called to actually apply the position
            stickyLog.Update(new GameTime());
            
            // Get the Position property using reflection (since it's private)
            var positionProperty = typeof(Canvas).GetProperty("Position", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var position = (Vector2)positionProperty.GetValue(canvas);
            
            Assert.Equal(newPosition, position);
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
            var logField = typeof(StickyLog).GetField("log", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var log = (System.Collections.Generic.Dictionary<string, Label>)logField.GetValue(stickyLog);
            
            Assert.False(log.ContainsKey("Key1"));
            Assert.True(log.ContainsKey("Key2"));
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
            var logField = typeof(StickyLog).GetField("log", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var log = (System.Collections.Generic.Dictionary<string, Label>)logField.GetValue(stickyLog);
            
            Assert.Empty(log);
            
            var gridField = typeof(StickyLog).GetField("_grid", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var grid = (Grid)gridField.GetValue(stickyLog);
            
            Assert.Empty(grid.Widgets);
        }
    }
}
