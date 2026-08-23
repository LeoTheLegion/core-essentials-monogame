using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Inputs;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene demonstrating fully data-driven, Unity-style anchored GUI.
/// The entire HUD (labels + buttons) is defined in <c>GuiAnchorDemo.xml</c> using plain
/// <see cref="GameObjectEntity"/> nodes composed of built-in components:
/// one root <c>CanvasComponent</c>, plus <c>AnchorComponent</c> + <c>LabelComponent</c>/<c>ButtonComponent</c>
/// on each child. No per-element game code is needed — this scene only wires the button
/// commands and drives the live-updating score label (see Issue #68).
/// </summary>
public class GuiAnchorDemoScene : Scene
{
    private int _score;
    private LabelComponent _scoreLabel;

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

        // Everything below the <Scene> root is data: entities, canvas, anchors and widgets.
        LoadEntitiesFromXml("GuiAnchorDemo.xml", entitySystem);

        // A camera so the world-space panel can be panned around (WASD) to show that its
        // anchored children stay pinned inside it in world space, not screen space.
        entitySystem.CreateEntity<CameraEntity>();

        UpdateLoadingProgress(0.7f, "Wiring button commands...");
        yield return null;

        // The only hand-written code in this scene: command wiring + the live score label.
        var addButton = FindButton(entitySystem, "addScoreButton");
        if (addButton != null)
            addButton.Clicked += () => UpdateScore(_score + 10);

        var resetButton = FindButton(entitySystem, "resetButton");
        if (resetButton != null)
            resetButton.Clicked += () => UpdateScore(0);

        _scoreLabel = entitySystem.FindById("scoreText")?.GetComponent<LabelComponent>();
        UpdateScore(0);

        // Escape returns to the original startup scene.
        Input.Keyboard.KeyReleased += OnKeyReleased;

        UpdateLoadingProgress(1.0f, "Anchored GUI scene ready!");
        Console.WriteLine("[GuiAnchorDemo] Scene loaded — every HUD element came from XML (anchors + offsets).");
        yield break;
    }

    public override void Unload()
    {
        base.Unload();
        Input.Keyboard.KeyReleased -= OnKeyReleased;
    }

    private void UpdateScore(int value)
    {
        _score = value;

        // Live pass-through (Issue #68): setting Text after attach updates the rendered widget immediately.
        if (_scoreLabel != null)
            _scoreLabel.Text = $"Score: {value}";

        Console.WriteLine($"[GuiAnchorDemo] Score = {value}");
    }

    private static ButtonComponent FindButton(EntitySystem system, string id) =>
        system.FindById(id)?.GetComponent<ButtonComponent>();

    private void OnKeyReleased(object sender, KeyboardEventArgs e)
    {
        if (e.Key == Keys.Escape)
            SceneManager.LoadScene(new PhysicsEntityScene());
    }
}
