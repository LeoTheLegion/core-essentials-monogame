using System;
using System.Collections;
using System.Linq;
using CoreEssentials.SceneManagement;
using CoreEssentials.GameSystems;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Types;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene to demonstrate the XML Layout Support provided by the GuiSerializer.
/// </summary>
public class XmlLayoutScene : Scene
{
    private IContentManager _contentWrapper;
    private IPanel _mainPanel;
    private IButton _exampleButton;
    private ILabel _statusLabel;

    protected override GameSystem[] LoadGameSystems()
    {
        return Array.Empty<GameSystem>();
    }

    protected override IEnumerator OnStartCoroutine()
    {
        _contentWrapper = new ContentManagerWrapper(SceneManager.Game.Content);
        UpdateLoadingProgress(0.1f, "Initializing XML Layout scene...");
        yield return null;

        // 1. Demo String-based layout
        string inlineXml = @"
        <Panel Width=""400"" Height=""300"" X=""100"" Y=""100"">
            <Label Text=""Welcome to XML Layouts!"" Width=""300"" X=""50"" Y=""20"" TextColor=""Yellow"" />
            <Button Text=""Click Me!"" Width=""150"" Height=""40"" X=""125"" Y=""200"" />
        </Panel>";

        _mainPanel = GuiSerializer.LoadPanelFromXml(inlineXml, _contentWrapper);
        
        _exampleButton = _mainPanel.Children.OfType<IButton>().FirstOrDefault();
        if (_exampleButton != null)
        {
            _exampleButton.Clicked += (btn) => 
            {
                Console.WriteLine("XML Button Clicked!");
                if (_statusLabel != null) _statusLabel.Text = "Button was clicked!";
            };
        }

        try 
        {
            var asset = new XMLAsset("layout/main.xml");
            asset.Load(_contentWrapper);
            var assetPanel = GuiSerializer.LoadPanelFromXml(asset, _contentWrapper);
            assetPanel.Position = new Vector2(600, 100);
            
            var assetButton = assetPanel.Children.OfType<IButton>().FirstOrDefault();
            if (assetButton != null)
            {
                assetButton.Clicked += (btn) => 
                {
                    Console.WriteLine("Asset XML Button Clicked!");
                    if (_statusLabel != null) _statusLabel.Text = "Asset button was clicked!";
                };
            }

            GUIManager.AddWidget(assetPanel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: XMLAsset demo failed (expected if layout/main.xml doesn't exist): {ex.Message}");
        }

        // Add the main inline panel to the canvas
        GUIManager.AddWidget(_mainPanel);

        UpdateLoadingProgress(1.0f, "XML Layout scene ready!");
    }

    public void OnExit()
    {
        // Cleanup widgets from the manager
        if (_mainPanel != null)
        {
            GUIManager.RemoveWidget(_mainPanel);
        }
    }
}
