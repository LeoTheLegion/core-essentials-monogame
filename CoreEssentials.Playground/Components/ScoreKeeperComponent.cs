using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Demonstrates declarative &lt;Bind&gt; command wiring: the score state lives in
/// this component, and the Add/Reset buttons in <c>GuiAnchorDemo.xml</c> bind their
/// <c>Clicked</c> events to its public methods — no FindById + subscribe code in the scene.
/// The label entity is injected from XML via a &lt;Reference&gt; element.
/// </summary>
public class ScoreKeeperComponent : EntityComponent
{
    /// <summary>Settable from XML via &lt;References&gt;&lt;Reference Name="ScoreLabel" .../&gt;.</summary>
    public Entity? ScoreLabel;

    private int _score;

    /// <summary>Bound to the "Add 10" button's Clicked event from XML.</summary>
    public void AddTen() => SetScore(_score + 10);

    /// <summary>Bound to the "Reset" button's Clicked event from XML.</summary>
    public void Reset() => SetScore(0);

    private void SetScore(int value)
    {
        _score = value;
        if (ScoreLabel is { } labelEntity && labelEntity.GetComponent<LabelComponent>() is { } label)
            label.Text = $"Score: {value}";

        Console.WriteLine($"[GuiAnchorDemo] Score = {value}");
    }
}
