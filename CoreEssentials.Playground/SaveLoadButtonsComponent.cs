using System;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.Playground;

/// <summary>
/// Declaratively reproduces the save/load GUI buttons that used to live in a scene subclass, so
/// the scene can be pure data. On attach it creates two text buttons (save and load) at
/// configurable positions/sizes, wires their clicks to the owning entity system's
/// <c>SaveState</c>/<c>LoadState</c>, and adds them to the GUI root. On detach it removes both
/// widgets so a scene unload leaves no dangling UI.
/// <code>
/// &lt;Component Type="SaveLoadButtonsComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="SaveFilePath" Value="PhysicsScene_Save.xml" /&gt;
///     &lt;Property Name="SaveButtonLabel" Value="Save Physics Scene" /&gt;
///     &lt;Property Name="LoadButtonLabel" Value="Load Physics Scene" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// Button creation, GUI add/remove, and the save/load calls are small <c>protected virtual</c>
/// seams so unit tests can observe them without a live GUI engine or entity system.
/// </summary>
public class SaveLoadButtonsComponent : EntityComponent
{
    /// <summary>The file path used for both saving and loading scene state.</summary>
    public string SaveFilePath { get; set; } = "PhysicsScene_Save.xml";

    /// <summary>The display text on the save button.</summary>
    public string SaveButtonLabel { get; set; } = "Save Physics Scene";

    /// <summary>The display text on the load button.</summary>
    public string LoadButtonLabel { get; set; } = "Load Physics Scene";

    /// <summary>Position of the save button (pixels).</summary>
    public Vector2 SaveButtonPosition { get; set; } = new(20, 20);

    /// <summary>Size of the save button (pixels).</summary>
    public Vector2 SaveButtonSize { get; set; } = new(200, 50);

    /// <summary>Position of the load button (pixels).</summary>
    public Vector2 LoadButtonPosition { get; set; } = new(20, 80);

    /// <summary>Size of the load button (pixels).</summary>
    public Vector2 LoadButtonSize { get; set; } = new(200, 50);

    private IButton? _saveButton;
    private IButton? _loadButton;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _saveButton = CreateSaveButton();
        if (_saveButton != null)
        {
            PositionAndSize(_saveButton, SaveButtonPosition, SaveButtonSize);
            _saveButton.Clicked += _ => Save();
            AddWidget(_saveButton);
        }

        _loadButton = CreateLoadButton();
        if (_loadButton != null)
        {
            PositionAndSize(_loadButton, LoadButtonPosition, LoadButtonSize);
            _loadButton.Clicked += _ => Load();
            AddWidget(_loadButton);
        }
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_saveButton != null)
        {
            RemoveWidget(_saveButton);
            _saveButton = null;
        }
        if (_loadButton != null)
        {
            RemoveWidget(_loadButton);
            _loadButton = null;
        }
    }

    // ── Testability seams ────────────────────────────────────────────────────────

    /// <summary>Creates the save button. Virtual so unit tests can inject a fake widget.</summary>
    protected virtual IButton? CreateSaveButton() => WidgetFactory.CreateTextButton(SaveButtonLabel);

    /// <summary>Creates the load button. Virtual so unit tests can inject a fake widget.</summary>
    protected virtual IButton? CreateLoadButton() => WidgetFactory.CreateTextButton(LoadButtonLabel);

    /// <summary>Adds a widget to the GUI root. Virtual so unit tests can observe the wiring.</summary>
    protected virtual void AddWidget(IWidget widget) => GUIManager.AddWidget(widget);

    /// <summary>Removes a widget from the GUI root. Virtual so unit tests can observe cleanup.</summary>
    protected virtual void RemoveWidget(IWidget widget) => GUIManager.RemoveWidget(widget);

    /// <summary>Saves the entity system's state to <see cref="SaveFilePath"/>.</summary>
    protected virtual void Save()
        => EntitySystem?.SaveState(SaveFilePath);

    /// <summary>Loads the entity system's state from <see cref="SaveFilePath"/>.</summary>
    protected virtual void Load()
        => EntitySystem?.LoadState(SaveFilePath);

    private static void PositionAndSize(IButton button, Vector2 position, Vector2 size)
    {
        button.Position = position;
        button.AutoWidth = false;
        button.Width = size.X;
        button.AutoHeight = false;
        button.Height = size.Y;
    }
}
