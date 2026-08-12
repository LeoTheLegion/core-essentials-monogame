using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.Scenes;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.Audio;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene to demonstrate the SpriteSheet functionality with a character display.
/// Demonstrates XML-based entity loading with unique IDs and cross-entity references.
/// </summary>
public class CharacterScene : Scene
{
    private string songID;
    private const string InfoText = "Press Q, W, E for sound effects | Z, X to change volume | Right Arrow for next scene | Or use the buttons on the left";
    private const string CharacterInfo = "Static Character (Left) | Animated Character (Right)";

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

        UpdateLoadingProgress(0.3f, "Loading entities from XML...");
        yield return null;

        EntitySystem entitySystem = GetGameSystem<EntitySystem>();

        // --- Demo: Load entities from XML with IDs ---
        string sceneXml = $@"
<Scene>
    <Entity Type=""CoreEssentials.Playground.CharacterEntity"" Id=""staticCharacter"">
        <Position X=""{graphics.PreferredBackBufferWidth / 4}"" Y=""{graphics.PreferredBackBufferHeight / 2}"" />
        <Tags>
            <Tag Name=""Character"" />
            <Tag Name=""Static"" />
        </Tags>
    </Entity>

    <Entity Type=""CoreEssentials.Playground.AnimatedCharacterEntity"" Id=""animatedCharacter"">
        <Position X=""{graphics.PreferredBackBufferWidth * 3 / 4}"" Y=""{graphics.PreferredBackBufferHeight / 2}"" />
        <Tags>
            <Tag Name=""Character"" />
            <Tag Name=""Animated"" />
        </Tags>
    </Entity>

    <Entity Type=""CoreEssentials.Playground.TextEntity"" Id=""infoText"">
        <Position X=""{graphics.PreferredBackBufferWidth / 2}"" Y=""20"" />
    </Entity>

    <Entity Type=""CoreEssentials.Playground.TextEntity"" Id=""characterInfoText"">
        <Position X=""{graphics.PreferredBackBufferWidth / 2}"" Y=""{graphics.PreferredBackBufferHeight - 40}"" />
    </Entity>
</Scene>";

        // Parse and load entities from XML
        var sceneElement = System.Xml.Linq.XDocument.Parse(sceneXml).Root;
        foreach (var entityElement in sceneElement.Elements("Entity"))
        {
            string typeName = entityElement.Attribute("Type")?.Value ?? throw new FormatException("Entity type is required");
            Type type = Type.GetType(typeName) ?? throw new FormatException($"Unknown entity type: {typeName}");

            // Create entity and apply XML properties (including ID)
            var entity = entitySystem.CreateEntityUnstarted(type);
            EntitySerializer.ApplyEntityProperties(entity, entityElement);
            entity.OnStart();
        }

        UpdateLoadingProgress(0.5f, "Looking up entities by ID...");
        yield return null;

        // --- Demo: Find entities by ID ---
        var staticChar = entitySystem.FindById("staticCharacter") as CharacterEntity;
        var animatedChar = entitySystem.FindById("animatedCharacter") as AnimatedCharacterEntity;
        var infoTextEntity = entitySystem.FindById("infoText") as TextEntity;
        var charInfoTextEntity = entitySystem.FindById("characterInfoText") as TextEntity;

        // Configure text entities using ID lookup
        if (infoTextEntity != null)
        {
            infoTextEntity.Text = InfoText;
            infoTextEntity.Color = Color.White;
            infoTextEntity.Alignment = TextEntity.TextAlignment.Center;
        }

        if (charInfoTextEntity != null)
        {
            charInfoTextEntity.Text = CharacterInfo;
            charInfoTextEntity.Color = Color.LightGreen;
            charInfoTextEntity.Alignment = TextEntity.TextAlignment.Center;
        }

        // --- Demo: EntityReference for cross-entity linking ---
        var reference = new EntityReference("staticCharacter");
        bool resolved = reference.Resolve(entitySystem.GetIdIndex());
        Console.WriteLine($"Reference to 'staticCharacter' resolved: {resolved}");

        if (resolved)
        {
            Console.WriteLine($"  -> Found entity at position: {reference.ResolvedEntity.Position}");
        }

        UpdateLoadingProgress(0.7f, "Creating UI buttons...");
        yield return null;

        // Create sound button entities
        entitySystem.CreateEntity<SoundButtonEntity>(
            new Vector2(100, 100),
            "footstep1_sound.xml",
            "Footstep 1"
        );

        entitySystem.CreateEntity<SoundButtonEntity>(
            new Vector2(100, 150),
            "footstep2_sound.xml",
            "Footstep 2"
        );

        entitySystem.CreateEntity<SoundButtonEntity>(
            new Vector2(100, 200),
            "footstep3_sound.xml",
            "Footstep 3"
        );

        // Create volume control buttons
        entitySystem.CreateEntity<VolumeButtonEntity>(
            new Vector2(100, 250),
            0.1f,
            "Volume: 10%"
        );

        entitySystem.CreateEntity<VolumeButtonEntity>(
            new Vector2(100, 300),
            1.0f,
            "Volume: 100%"
        );

        // Register input handler
        Input.Keyboard.KeyReleased += Reset();
        Input.Keyboard.KeyReleased += PlaySound();

        UpdateLoadingProgress(1.0f, "Scene ready!");
        Console.WriteLine("Character scene loaded successfully with XML IDs!");

        songID = AudioManager.Instance.PlaySound("song1_sound.xml");
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

    private static EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> PlaySound()
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
        };
    }
}
