using CoreEssentials.Camera;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Assets;
using CoreEssentials.Inputs;
using System.Diagnostics; // Added for Debug.WriteLine

namespace CoreEssentials.Playground
{
    public class CameraScene : Scene
    {
        private CameraEntity cameraEntity;
        private PlayerEntity player; 
        private TextEntity cameraInfoText;
        
        // Event handlers for input
        // Camera key press/release are now handled by CameraEntity
        private EventHandler<KeyboardEventArgs> keyReleaseHandlerForScene; // CoreEssentials-owned KeyboardEventArgs

        protected override GameSystem[] LoadGameSystems()
        {
            return new GameSystem[]
            {
                new EntitySystem()
            };
        }

        protected override IEnumerator OnStartCoroutine()
        {
            UpdateLoadingProgress(0.1f, "Initializing camera scene...");
            yield return null;
            
            // Get the entity system
            EntitySystem entitySystem = GetGameSystem<EntitySystem>();
            
            UpdateLoadingProgress(0.3f, "Setting up camera...");
            yield return null;

            // Create a camera entity
            cameraEntity = entitySystem.CreateEntity<CameraEntity>();
                    
            // Create a player entity that we'll track with the camera
            player = entitySystem.CreateEntity<PlayerEntity>(new Vector2(400, 300)); // Changed to PlayerEntity
            
            // Add camera info text
            cameraInfoText = entitySystem.CreateEntity<TextEntity>(
                new Vector2(10, 10),
                "Camera Controls:\n" +
                "WASD: Move Camera\n" +
                "Q/E: Zoom In/Out\n" +
                "R: Reset Camera\n" +
                "F: Follow Player (Toggle)\n" +
                "Arrow Keys: Move Player",
                Color.White,
                TextEntity.TextAlignment.Left);
            
            // Register input handlers
            // keyPressHandler is removed as CameraEntity handles its own WASD/QE
            keyReleaseHandlerForScene = HandleSceneKeyRelease(); // Renamed handler
            
            // Input.Keyboard.KeyPressed is no longer subscribed to by CameraScene for camera/player movement
            Input.Keyboard.KeyReleased += keyReleaseHandlerForScene; // Subscribe the scene-specific key release handler
            
            // Set initial camera info text
            UpdateCameraInfoText();
            
            yield return null;
        }
        
        public override void Unload()
        {
            base.Unload();
            // Unregister event handlers to prevent memory leaks
            // Input.Keyboard.KeyPressed -= keyPressHandler; // CameraEntity and PlayerEntity handle their own KeyPressed
            if (keyReleaseHandlerForScene != null) // Check if the handler was assigned
            {
                Input.Keyboard.KeyReleased -= keyReleaseHandlerForScene;
            }
        }
        
        // Update camera info text based on follow mode
        private void UpdateCameraInfoText()
        {
            cameraInfoText.Text = "Camera Controls:\n" +
                "WASD: Move Camera\n" +
                "Q/E: Zoom In/Out\n" +
                "R: Reset Camera\n" +
                "F: Follow Player (" + (cameraEntity.FollowingTarget ? "ON" : "OFF") + ")\n" +
                "Arrow Keys: Move Player";
        }
        
        // Handle continuous key presses (for movement)   
        // HandleKeyPress method is removed as its logic is now in CameraEntity and PlayerEntity

        private EventHandler<KeyboardEventArgs> HandleSceneKeyRelease() // CoreEssentials-owned KeyboardEventArgs
        {
            return (sender, args) =>
            {
                // Handle toggle of player following with F key
                if (args.Key == Keys.F)
                {
                    cameraEntity.ToggleFollow(player);
                    UpdateCameraInfoText();
                }
                
                // Transition to CharacterScene with + key (either numpad or main keyboard)
                if (args.Key == Keys.Add || args.Key == Keys.OemPlus) 
                {
                    Debug.WriteLine("Plus key pressed, attempting to load CharacterScene..."); // Added debug message
                    SceneManager.LoadScene(new CharacterScene());
                }
                
                // Reset camera with R key is now handled by CameraEntity
            };
        }
    }
}
