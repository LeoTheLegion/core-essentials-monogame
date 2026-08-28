using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.Inputs;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene demonstrating fully data-driven, Unity-style anchored GUI.
/// The entire HUD (labels + buttons) is defined in <c>GuiAnchorDemo.xml</c> using plain
/// <see cref="GameObjectEntity"/> nodes composed of built-in components:
/// one root <c>CanvasComponent</c>, plus <c>AnchorComponent</c> + <c>LabelComponent</c>/<c>ButtonComponent</c>
/// on each child. Button behavior is wired declaratively with &lt;Bind&gt; elements
/// — this scene contains no FindById + subscribe code at all.
/// </summary>
public class GuiAnchorDemoScene : Scene
{

    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]
        {
            new EntitySystem()
        };
    }

    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.2f, "Loading anchored GUI scene...");
        yield return null;

        var entitySystem = GetGameSystem<EntitySystem>();

        // Everything below the <Scene> root is data: entities, canvas, anchors, widgets,
        // and the button command wiring (<Bind Event="Clicked" Command="..."/> in the XML).
        var factory = new DefaultComponentFactory();
        factory.Register("ScoreKeeperComponent", () => new ScoreKeeperComponent());
        LoadEntitiesFromXml("GuiAnchorDemo.xml", entitySystem, factory);

        // A camera so the world-space panel can be panned around (WASD) to show that its
        // anchored children stay pinned inside it in world space, not screen space.
        var cameraEntity = entitySystem.CreateEntity<CameraEntity>();
        cameraEntity.CameraSpeed = 300f; // world units/second — the default of 1 is imperceptible

        UpdateLoadingProgress(0.7f, "Scene ready...");
        yield return null;

        // Escape returns to the original startup scene.
        Input.Keyboard.KeyReleased += OnKeyReleased;

        UpdateLoadingProgress(1.0f, "Anchored GUI scene ready!");
        Console.WriteLine("[GuiAnchorDemo] Scene loaded — every HUD element and its button wiring came from XML.");
        yield break;
    }

    public override void Unload()
    {
        base.Unload();
        Input.Keyboard.KeyReleased -= OnKeyReleased;
    }

    private void OnKeyReleased(object sender, KeyboardEventArgs e)
    {
        if (e.Key == Keys.Escape)
            SceneManager.LoadScene(new PhysicsEntityScene());
    }
}
