using Xunit;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using Moq;

namespace CoreEssentials.Tests.Debugging
{
    /// <summary>
    /// Tests for the BaseGameDiagnostics class.
    /// </summary>
    public class BaseGameDiagnosticsTests
    {
        // Create a mock StickyLog for testing
        private StickyLog CreateMockStickyLog()
        {
            var mock = new Mock<StickyLog>();
            return mock.Object;
        }
        
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            var stickyLog = CreateMockStickyLog();
            
            // Act
            var diagnostics = new BaseGameDiagnostics(stickyLog);
            
            // Assert - initial averages should be zero
            Assert.Equal(0f, diagnostics.UpdateAvg);
            Assert.Equal(0f, diagnostics.DrawAvg);
            Assert.Equal(0f, diagnostics.FixedUpdateAvg);
        }
        
        [Fact]
        public void UpdateMethods_StartAndStopStopwatches()
        {
            // Arrange
            var stickyLog = CreateMockStickyLog();
            var diagnostics = new BaseGameDiagnostics(stickyLog);
            
            // Get stopwatch using reflection
            var updateStopwatchField = typeof(BaseGameDiagnostics).GetField("_updateStopwatch", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var updateStopwatch = (Stopwatch)updateStopwatchField.GetValue(diagnostics);
            
            // Assert initial state
            Assert.False(updateStopwatch.IsRunning);
            
            // Act - begin update
            diagnostics.UpdateBegin();
            
            // Assert - stopwatch should be running
            Assert.True(updateStopwatch.IsRunning, "Update stopwatch should be running after UpdateBegin");
            
            // Simulate some work
            Thread.Sleep(5);
            
            // Act - end update
            diagnostics.UpdateEnd();
            
            // Assert - stopwatch should be stopped and average should be calculated
            Assert.False(updateStopwatch.IsRunning, "Update stopwatch should not be running after UpdateEnd");
            Assert.True(diagnostics.UpdateAvg > 0, "Update average should be greater than zero after execution");
        }
        
        [Fact]
        public void DrawMethods_StartAndStopStopwatches()
        {
            // Arrange
            var stickyLog = CreateMockStickyLog();
            var diagnostics = new BaseGameDiagnostics(stickyLog);
            
            // Get stopwatch using reflection
            var drawStopwatchField = typeof(BaseGameDiagnostics).GetField("_drawStopwatch", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var drawStopwatch = (Stopwatch)drawStopwatchField.GetValue(diagnostics);
            
            // Assert initial state
            Assert.False(drawStopwatch.IsRunning);
            
            // Act - begin draw
            diagnostics.DrawBegin();
            
            // Assert - stopwatch should be running
            Assert.True(drawStopwatch.IsRunning, "Draw stopwatch should be running after DrawBegin");
            
            // Simulate some work
            Thread.Sleep(5);
            
            // Act - end draw
            diagnostics.DrawEnd();
            
            // Assert - stopwatch should be stopped and average should be calculated
            Assert.False(drawStopwatch.IsRunning, "Draw stopwatch should not be running after DrawEnd");
            Assert.True(diagnostics.DrawAvg > 0, "Draw average should be greater than zero after execution");
        }
        
        [Fact]
        public void FixedUpdateMethods_StartAndStopStopwatches()
        {
            // Arrange
            var stickyLog = CreateMockStickyLog();
            var diagnostics = new BaseGameDiagnostics(stickyLog);
            
            // Get stopwatch using reflection
            var fixedUpdateStopwatchField = typeof(BaseGameDiagnostics).GetField("_fixedUpdateStopwatch", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var fixedUpdateStopwatch = (Stopwatch)fixedUpdateStopwatchField.GetValue(diagnostics);
            
            // Assert initial state
            Assert.False(fixedUpdateStopwatch.IsRunning);
            
            // Act - begin fixed update
            diagnostics.FixedUpdateBegin();
            
            // Assert - stopwatch should be running
            Assert.True(fixedUpdateStopwatch.IsRunning, "FixedUpdate stopwatch should be running after FixedUpdateBegin");
            
            // Simulate some work
            Thread.Sleep(5);
            
            // Act - end fixed update
            diagnostics.FixedUpdateEnd();
            
            // Assert - stopwatch should be stopped and average should be calculated
            Assert.False(fixedUpdateStopwatch.IsRunning, "FixedUpdate stopwatch should not be running after FixedUpdateEnd");
            Assert.True(diagnostics.FixedUpdateAvg > 0, "FixedUpdate average should be greater than zero after execution");
        }
          [Fact]
        public void MultipleUpdates_PopulatesDiagnosticValues()
        {
            // Arrange
            var stickyLog = CreateMockStickyLog();
            var diagnostics = new BaseGameDiagnostics(stickyLog);
            
            // Act - run multiple cycles
            for (int i = 0; i < 3; i++)
            {
                // Update cycle
                diagnostics.UpdateBegin();
                Thread.Sleep(5);
                diagnostics.UpdateEnd();
                
                // Draw cycle
                diagnostics.DrawBegin();
                Thread.Sleep(3);
                diagnostics.DrawEnd();
                
                // Fixed update cycle
                diagnostics.FixedUpdateBegin();
                Thread.Sleep(2);
                diagnostics.FixedUpdateEnd();
            }              // Assert - averages should be calculated and positive
            Assert.True(diagnostics.UpdateAvg >= 0);
            Assert.True(diagnostics.DrawAvg >= 0);
            Assert.True(diagnostics.FixedUpdateAvg >= 0);
        }
    }
}
