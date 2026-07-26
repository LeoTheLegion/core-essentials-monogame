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
    protected override GameSystem[] LoadGameSystems()
    {
        return Array.Empty<GameSystem>();
    }

    protected override IEnumerator OnStartCoroutine()
    {
        var contentWrapper = new ContentManagerWrapper(SceneManager.Game.Content);
        UpdateLoadingProgress(0.1f, "Initializing XML Layout scene...");
        yield return null;

        // 1. Demo String-based layout
        string inlineXml = @"
        <Panel Width=""400"" Height=""300"" X=""100"" Y=""100"">
            <Label Text=""Welcome to XML Layouts!"" Width=""300"" X=""50"" Y=""20"" TextColor=""Yellow"" />
            <Button Text=""Click Me!"" Width=""150"" Height=""40"" X=""125"" Y=""200"" />
        </Panel>";

        var mainPanel = GuiSerializer.LoadPanelFromXml(inlineXml, contentWrapper);
        
        var exampleButton = mainPanel.Children.OfType<IButton>().FirstOrDefault();
        if (exampleButton != null)
        {
            exampleButton.Clicked += (btn) => 
            {
                Console.WriteLine("XML Button Clicked!");
            };
        }

        try 
        {
            var asset = new XMLAsset("layout/main.xml");
            asset.Load(contentWrapper);
            var assetPanel = GuiSerializer.LoadPanelFromXml(asset, contentWrapper);
            assetPanel.Position = new Vector2(600, 100);
            
            var assetButton = assetPanel.Children.OfType<IButton>().FirstOrDefault();
            if (assetButton != null)
            {
                assetButton.Clicked += (btn) => 
                {
                    Console.WriteLine("Asset XML Button Clicked!");
                };
            }

            GUIManager.AddWidget(assetPanel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: XMLAsset demo failed (expected if layout/main.xml doesn't exist): {ex.Message}");
        }

        // Add the main inline panel to the canvas
        GUIManager.AddWidget(mainPanel);

        UpdateLoadingProgress(1.0f, "XML Layout scene ready!");
    }
}
