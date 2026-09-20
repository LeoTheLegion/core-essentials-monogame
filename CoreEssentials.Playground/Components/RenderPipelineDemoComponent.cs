using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Rendering;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Registers a full-screen <b>vignette post pass</b> with the static <see cref="RenderPipeline"/> so
/// the frame gets a subtle edge darkening after the scene + GUI render. This demonstrates the
/// post-pass API from data: the effect is resolved through the declarative <c>EffectAsset</c> (the same
/// loader path a per-sprite <c>SpriteComponent.EffectAsset</c> uses), then handed to
/// <see cref="RenderPipeline.AddPostPass"/>. It is an additive overlay, so no render target is needed.
/// <code>
/// &lt;EntityDefinition Type="...GameObjectEntity" Id="pipelineDemo"&gt;
///   &lt;Components&gt;&lt;Component Type="RenderPipelineDemoComponent" /&gt;&lt;/Components&gt;
/// &lt;/EntityDefinition&gt;
/// </code>
/// The pass is removed on detach, so unloading this scene restores the pipeline to its default (no-op)
/// state — important because <see cref="RenderPipeline"/> is global and scenes are loaded sequentially.
/// </summary>
public class RenderPipelineDemoComponent : EntityComponent
{
    /// <summary>The asset name of the vignette effect (content-pipeline key, e.g. "Effects/Vignette").</summary>
    public string EffectAsset { get; set; } = "Effects/Vignette";

    private Microsoft.Xna.Framework.Graphics.Effect? _effect;

    /// <inheritdoc />
    public override void OnAttach()
    {
        try
        {
            _effect = AssetManager.LoadAsset<EffectAsset>(EffectAsset).Effect;
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"[RenderPipelineDemoComponent] Could not load effect asset '{EffectAsset}': {ex.Message}");
            return;
        }

        // Additive overlay — drawn over the composited frame, no render target required.
        RenderPipeline.AddPostPass(_effect);
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_effect != null)
        {
            RenderPipeline.RemovePostPass(_effect);
            _effect = null;
        }
    }
}
