using System;
using System.Collections;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene to demonstrate the SpriteSheet functionality with a character display.
/// Demonstrates XML-based entity loading using templates and EntitySerializer.LoadSceneFromXml.
/// </summary>
public class CharacterScene : Scene
{
    private string songID;

    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]
        {
            new EntitySystem()
        };
    }

    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.1f, "Initializing character scene...");
        yield return null;

        GraphicsDeviceManager graphics = SceneManager.Game.Graphics;
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();

        UpdateLoadingProgress(0.3f, "Registering templates...");
        yield return null;

        EntitySystem entitySystem = GetGameSystem<EntitySystem>();

        // Register reusable entity templates
        entitySystem.RegisterTemplate("TextPrefab", "TextTemplate.xml");
        entitySystem.RegisterTemplate("SoundButtonPrefab", "SoundButtonTemplate.xml");
        entitySystem.RegisterTemplate("VolumeButtonPrefab", "VolumeButtonTemplate.xml");

        UpdateLoadingProgress(0.5f, "Loading entities from XML...");
        yield return null;

        // Load all entities from scene definition (uses EntitySerializer.LoadSceneFromXml)
        var sceneAsset = AssetManager.LoadAsset<CoreEssentials.Assets.XMLAsset>("CharacterScene.xml");
        var entities = LoadEntitiesFromXml(sceneAsset, entitySystem);

        UpdateLoadingProgress(0.7f, "Configuring entities by ID...");
        yield return null;

        // Configure text entities using ID lookup
        var infoTextEntity = entitySystem.FindById("infoText") as TextEntity;
        if (infoTextEntity != null)
        {
            infoTextEntity.Text = "Press Q, W, E for sound effects | Z, X to change volume | Right Arrow for next scene | Or use the buttons on the left";
            infoTextEntity.Color = Color.White;
            infoTextEntity.Alignment = TextEntity.TextAlignment.Center;
        }

        var charInfoTextEntity = entitySystem.FindById("characterInfoText") as TextEntity;
        if (charInfoTextEntity != null)
        {
            charInfoTextEntity.Text = "Static Character (Left) | Animated Character (Right)";
            charInfoTextEntity.Color = Color.LightGreen;
            charInfoTextEntity.Alignment = TextEntity.TextAlignment.Center;
        }

        // Configure sound buttons using ID lookup
        var footstep1Btn = entitySystem.FindById("footstep1Button") as SoundButtonEntity;
        if (footstep1Btn != null) footstep1Btn.Configure("footstep1_sound.xml", "Footstep 1");

        var footstep2Btn = entitySystem.FindById("footstep2Button") as SoundButtonEntity;
        if (footstep2Btn != null) footstep2Btn.Configure("footstep2_sound.xml", "Footstep 2");

        var footstep3Btn = entitySystem.FindById("footstep3Button") as SoundButtonEntity;
        if (footstep3Btn != null) footstep3Btn.Configure("footstep3_sound.xml", "Footstep 3");

        // Configure volume buttons using ID lookup
        var volumeLowBtn = entitySystem.FindById("volumeLowButton") as VolumeButtonEntity;
        if (volumeLowBtn != null) volumeLowBtn.Configure(0.1f, "Volume: 10%");

        var volumeHighBtn = entitySystem.FindById("volumeHighButton") as VolumeButtonEntity;
        if (volumeHighBtn != null) volumeHighBtn.Configure(1.0f, "Volume: 100%");

        UpdateLoadingProgress(0.9f, "Scene ready!");
        yield return null;

        // Register input handler
        Input.Keyboard.KeyReleased += Reset();
        Input.Keyboard.KeyReleased += PlaySound();

        // Setup debug visualization — toggle with F3
        entitySystem.DebugMode = true;
        entitySystem.DebugConfig.ShowEntityBounds = true;
        entitySystem.DebugConfig.ShowEntityIds = true;
        entitySystem.DebugConfig.ShowEntityTags = true;
        entitySystem.DebugConfig.ShowEntityHierarchy = true;
        entitySystem.DebugConfig.ShowEntityPosition = true;
        entitySystem.DebugFont = AssetManager.LoadAsset<FontAsset>("base");

        UpdateLoadingProgress(1.0f, "Scene ready!");
        Console.WriteLine($"Character scene loaded with {entities.Count} entities from XML! (Debug mode ON — press F3 to toggle)");

        songID = AudioManager.Instance.PlaySound("song1_sound.xml");
    }

    /// <summary>
    /// Pauses or resumes this scene's background music when the application loses or regains focus.
    /// </summary>
    public override void OnApplicationPause(bool paused)
    {
        base.OnApplicationPause(paused);

        if (string.IsNullOrEmpty(songID))
            return;

        if (paused)
            AudioManager.Instance.PauseSound(songID);
        else
            AudioManager.Instance.ResumeSound(songID);
    }

    public override void Unload()
    {
        base.Unload();
        Input.Keyboard.KeyReleased -= Reset();
        Input.Keyboard.KeyReleased -= PlaySound();
    }

    private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> Reset()
    {
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Add || args.Key == Keys.OemPlus)
            {
                AudioManager.Instance.StopSound(songID);
                // Use SceneManager property directly here to get the current reference at the time of the event
                SceneManager.LoadScene(new PhysicsEntityScene());
            }
        };
    }

    private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> PlaySound()
    {
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Q)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep1_sound.xml");
                Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.W)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep2_sound.xml");
                Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.E)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep3_sound.xml");
                Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Z)
            {
                AudioManager.Instance.SetMasterVolume(0.1f);
                Console.WriteLine("Volume set to 10%");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.X)
            {
                AudioManager.Instance.SetMasterVolume(1.0f);
                Console.WriteLine("Volume set to 100%");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.F3)
            {
                var entitySystem = GetGameSystem<EntitySystem>();
                entitySystem.DebugMode = !entitySystem.DebugMode;
                Console.WriteLine($"Debug mode: {(entitySystem.DebugMode ? "ON" : "OFF")}");
            }
        };
    }
}
