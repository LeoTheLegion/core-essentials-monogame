using CoreEssentials.GameSystems;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreEssentials.SceneManagement
{
    /// <summary>
    /// A simple loading screen scene that displays loading progress.
    /// This scene is designed to be shown during transitions between other scenes.
    /// </summary>
    public class LoadingScene : Scene
    {
        private string _loadingText = "Loading...";
        private Color _backgroundColor = Color.Black;
        private Color _loadingBarColor = Color.White;
        private Color _textColor = Color.White;
        
        /// <summary>
        /// Sets the text to display on the loading screen.
        /// </summary>
        public string LoadingText 
        { 
            get => _loadingText;
            set => _loadingText = value;
        }
        
        /// <summary>
        /// Sets the background color of the loading screen.
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }
        
        /// <summary>
        /// Sets the color of the loading progress bar.
        /// </summary>
        public Color LoadingBarColor
        {
            get => _loadingBarColor;
            set => _loadingBarColor = value;
        }
        
        /// <summary>
        /// Sets the color of the loading text.
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

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
            _loadingText = loadingText;
            _backgroundColor = backgroundColor;
            _loadingBarColor = loadingBarColor;
            _textColor = textColor;
        }
        
        protected override GameSystem[] LoadGameSystems()
        {
            // Loading screen doesn't need any game systems
            return new GameSystem[0];
        }

        protected override void onStart()
        {
            // Initial setup, kept simple for fast loading
            Debug.Console.WriteLine("Loading screen initialized");
        }
        
        /// <summary>
        /// The loading scene loads very quickly since it has minimal initialization.
        /// </summary>
        protected override IEnumerator OnStartCoroutine()
        {
            // Update progress immediately to 100% since loading screen is simple
            _loadingProgress = 1.0f;
            yield break;
        }
        
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            
            // Get screen dimensions
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;
            
            // Clear the background
            graphicsDevice.Clear(_backgroundColor);
            
            spriteBatch.Begin();
            
            // Draw loading progress bar if we're transitioning to another scene
            if (SceneManager != null && SceneManager.NextScene != null)
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
                Debug.Primitives.DrawRectangle(spriteBatch, barBg, _loadingBarColor.WithAlpha(0.5f));
                
                // Fill the progress bar
                Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
                pixel.SetData(new[] { _loadingBarColor });
                spriteBatch.Draw(pixel, barFill, _loadingBarColor);
                
                // Draw percentage text
                string percentText = $"{Math.Floor(progress * 100)}%";
                Vector2 percentSize = new Vector2(8 * percentText.Length, 16); // Simple text size approximation
                Vector2 percentPos = new Vector2(
                    screenWidth / 2 - percentSize.X / 2,
                    barBg.Y + barBg.Height + 10
                );
                
                // Simple text rendering (in a real game, use SpriteFont)
                DrawSimpleText(spriteBatch, percentText, percentPos, _textColor);
            }
            
            // Draw loading text
            Vector2 textSize = new Vector2(8 * _loadingText.Length, 16); // Simple text size approximation
            Vector2 textPos = new Vector2(
                screenWidth / 2 - textSize.X / 2,
                screenHeight / 2 - textSize.Y / 2
            );
            
            // Simple text rendering (in a real game, use SpriteFont)
            DrawSimpleText(spriteBatch, _loadingText, textPos, _textColor);
            
            spriteBatch.End();
        }
        
        /// <summary>
        /// Simple utility to draw text without a SpriteFont.
        /// In a real game, you would use a SpriteFont instead.
        /// </summary>
        private void DrawSimpleText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            // This is a placeholder - in a real game you would use:
            // spriteBatch.DrawString(font, text, position, color);
            
            // For the placeholder, we'll draw rectangles representing characters
            for (int i = 0; i < text.Length; i++)
            {
                Rectangle charRect = new Rectangle(
                    (int)position.X + i * 8,
                    (int)position.Y,
                    6,
                    10
                );
                Debug.Primitives.DrawRectangle(spriteBatch, charRect, color);
            }
        }
    }

    public static class ColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.R, color.G, color.B, (byte)(255 * alpha));
        }
    }
}