using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreEssentials.Scenes
{
    /// <summary>
    /// A simple loading screen scene that displays loading progress.
    /// This scene is designed to be shown during transitions between other scenes.
    /// </summary>
    public class LoadingScene : Scene
    {
        /// <summary>
        /// Gets or sets the text to display on the loading screen.
        /// </summary>
        public string LoadingText { get; set; } = "Loading...";

        /// <summary>
        /// Gets or sets the background color of the loading screen.
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the color of the loading progress bar.
        /// </summary>
        public Color LoadingBarColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the color of the loading text.
        /// </summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>
        /// Creates a new instance of the LoadingScene class with default settings.
        /// </summary>
        public LoadingScene()
        {
            // Default constructor
        }
        
        /// <summary>
        /// Creates a new instance of the LoadingScene class with custom settings.
        /// </summary>
        /// <param name="loadingText">The text to display on the loading screen.</param>
        /// <param name="backgroundColor">The background color of the loading screen.</param>
        /// <param name="loadingBarColor">The color of the loading progress bar.</param>
        /// <param name="textColor">The color of the loading text.</param>
        public LoadingScene(string loadingText, Color backgroundColor, Color loadingBarColor, Color textColor)
        {
            LoadingText = loadingText;
            BackgroundColor = backgroundColor;
            LoadingBarColor = loadingBarColor;
            TextColor = textColor;
        }
        
        /// <summary>
        /// Loads the game systems required for the loading screen. The loading screen doesn't require any game systems, so this method returns an empty array.
        /// </summary>
        /// <returns>An array of game systems.</returns>
        protected override GameSystem[] LoadGameSystems()
        {
            // Loading screen doesn't need any game systems
            return Array.Empty<GameSystem>();
        }
        
        /// <summary>
        /// The loading scene loads very quickly since it has minimal initialization.
        /// </summary>
        protected override IEnumerator OnStartCoroutine()
        {
            Console.WriteLine("Loading screen initialized");
            // Update progress immediately to 100% since loading screen is simple
            _loadingProgress = 1.0f;
            yield break;
        }
        
        /// <summary>
        /// Draws the loading screen, including the loading text and progress bar.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            
            // Get screen dimensions
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;
            
            // Clear the background
            graphicsDevice.Clear(BackgroundColor);
            
            spriteBatch.Begin();
            
            // Draw loading progress bar if we're transitioning to another scene
            if (SceneManager != null && SceneManager.PendingScene != null)
            {
                float progress = SceneManager.TransitionProgress;
                
                // Draw progress bar background
                Rectangle barBg = new Rectangle(
                    screenWidth / 4,
                    screenHeight / 2 + 40,
                    screenWidth / 2,
                    20
                );
                
                // Draw progress bar fill
                Rectangle barFill = new Rectangle(
                    barBg.X,
                    barBg.Y,
                    (int)(barBg.Width * progress),
                    barBg.Height
                );
                
                // Draw the bars
                CoreEssentials.Debugging.Debug.Primitives.DrawRectangle(spriteBatch, barBg, LoadingBarColor.WithAlpha(0.5f));
                
                // Fill the progress bar
                Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
                pixel.SetData(new[] { LoadingBarColor });
                spriteBatch.Draw(pixel, barFill, LoadingBarColor);
                
                // Draw percentage text
                string percentText = $"{Math.Floor(progress * 100)}%";
                Vector2 percentSize = new Vector2(8 * percentText.Length, 16); // Simple text size approximation
                Vector2 percentPos = new Vector2(
                    screenWidth / 2 - percentSize.X / 2,
                    barBg.Y + barBg.Height + 10
                );
                
                // Simple text rendering (in a real game, use SpriteFont)
                DrawSimpleText(spriteBatch, percentText, percentPos, TextColor);
            }
            
            // Draw loading text
            Vector2 textSize = new Vector2(8 * LoadingText.Length, 16); // Simple text size approximation
            Vector2 textPos = new Vector2(
                screenWidth / 2 - textSize.X / 2,
                screenHeight / 2 - textSize.Y / 2
            );
            
            // Simple text rendering (in a real game, use SpriteFont)
            DrawSimpleText(spriteBatch, LoadingText, textPos, TextColor);
            
            spriteBatch.End();
        }
        
        /// <summary>
        /// Simple utility to draw text without a SpriteFont.
        /// In a real game, you would use a SpriteFont instead.
        /// </summary>
        private static void DrawSimpleText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            // For the placeholder, we'll draw rectangles representing characters
            for (int i = 0; i < text.Length; i++)
            {
                Rectangle charRect = new Rectangle(
                    (int)position.X + i * 8,
                    (int)position.Y,
                    6,
                    10
                );
                CoreEssentials.Debugging.Debug.Primitives.DrawRectangle(spriteBatch, charRect, color);
            }
        }
    }
    /// <summary>
    /// Extension methods for the Color struct.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns a new Color with the specified alpha value.
        /// </summary>
        /// <param name="color">The original color.</param>
        /// <param name="alpha">The alpha value (0.0 to 1.0).</param>
        /// <returns>A new Color with the specified alpha.</returns>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.R, color.G, color.B, (byte)(255 * alpha));
        }
    }
}