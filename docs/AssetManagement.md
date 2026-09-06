# Asset Management

The Asset Management system in CoreEssentials-MonoGame simplifies the loading, caching, and use of various game assets such as textures, sprites, audio, fonts, and custom XML data.

## Key Components

### AssetManager

The `AssetManager` class is the central static component for asset loading and management:

```csharp
// Load assets through the static AssetManager class
Texture2DAsset textureAsset = AssetManager.LoadAsset<Texture2DAsset>("character_malePerson_sheetHD");

// Load a sprite that references a sprite sheet
Sprite sprite = AssetManager.LoadAsset<Sprite>("character_sprite.xml");

// Load an animated sprite (same Sprite type — a one-frame sprite is just a static sprite)
Sprite animSprite = AssetManager.LoadAsset<Sprite>("character_anim_walk.xml");

// Load audio
AudioClip sound = AssetManager.LoadAsset<AudioClip>("footstep1_sound.xml");

// Load a font
FontAsset font = AssetManager.LoadAsset<FontAsset>("base");
```

### EffectAsset

The `EffectAsset` class is a wrapper for MonoGame's `Effect` class, used for custom shaders.

```csharp
// Load an effect asset (custom shader)
EffectAsset customEffectAsset = AssetManager.LoadAsset<EffectAsset>("MyCustomShader");

// Access the underlying Effect object
Effect shader = customEffectAsset.Effect;

// Apply the shader in your draw call
// spriteBatch.Begin(effect: shader);
// ... draw objects ...
// spriteBatch.End();
```

## Sprite Management

CoreEssentials provides robust sprite and animation support:

### Sprite

The `Sprite` class represents a single image or a part of a texture, and is loaded from XML:

```csharp
// Load a sprite from XML definition
Sprite sprite = AssetManager.LoadAsset<Sprite>("character_sprite.xml");

// Draw the sprite with default scale (1.0)
sprite.Draw(
    spriteBatch,
    position,
    Color.White,
    0f,
    SpriteEffects.None,
    0f
);

// Draw the sprite with a scale factor (2x size)
sprite.Draw(
    spriteBatch,
    position,
    Color.White,
    0f,
    2.0f,  // Scale the sprite to twice its size
    SpriteEffects.None,
    0f
);

// Draw the sprite with non-uniform scaling (stretched)
sprite.Draw(
    spriteBatch,
    position,
    Color.White,
    0f,
    new Vector2(1.5f, 0.8f),  // Wider but shorter
    SpriteEffects.None,
    0f
);
```

For more details on sprite scaling, see the [SpriteScaling.md](SpriteScaling.md) documentation.

### SpriteSheet

The `SpriteSheet` class manages sprite atlases and defines frames:

```csharp
// Load a sprite sheet from XML definition
SpriteSheet sheet = AssetManager.LoadAsset<SpriteSheet>("character_sheet.xml");

// Sprite sheets are typically used by the Sprite class
// and not directly manipulated
```

## Text Rendering with FontAsset

CoreEssentials provides font management through the `FontAsset` class:

### FontAsset

The `FontAsset` class represents a SpriteFont resource for rendering text:

```csharp
// Load a font asset
FontAsset fontAsset = assetManager.LoadAsset<FontAsset>("base");

// Use the font in a SpriteBatch
spriteBatch.DrawString(fontAsset.Font, "Hello, World!", new Vector2(100, 100), Color.White);

// Measure text width for positioning
float textWidth = fontAsset.MeasureString("Hello, World!");
Vector2 position = new Vector2(screenWidth / 2 - textWidth / 2, 100); // Center text horizontally

// Get full text dimensions as a Vector2
Vector2 textSize = fontAsset.MeasureStringVector("Hello, World!");
Vector2 center = new Vector2(screenWidth / 2 - textSize.X / 2, 
                             screenHeight / 2 - textSize.Y / 2); // Center text on screen
```

### Using MonoGame SpriteFont

The FontAsset class uses MonoGame's built-in SpriteFont system. Font files should be added to your Content project as `.spritefont` files and processed by the MonoGame Content Pipeline:

```xml
<!-- Example base.spritefont (this is processed by MonoGame's content pipeline) -->
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:Graphics="Microsoft.Xna.Framework.Content.Pipeline.Graphics">
  <Asset Type="Graphics:FontDescription">
    <FontName>ComicMono.ttf</FontName>
    <Size>14</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Regular</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
```

### Text Alignment Example

The `TextComponent` in the playground demonstrates how to use a `FontAsset` with different alignment options. It is a plain `IDrawableComponent`, so it can be attached to any entity (typically a behavior-free `GameObjectEntity`) and declared purely from scene/prefab XML:

```csharp
public class TextComponent : EntityComponent, IDrawableComponent
{
    private FontAsset? _font;

    public string Text { get; set; } = "";
    public Color Color { get; set; } = Color.White;
    public enum TextAlignment { Left, Center, Right }
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    public override void OnAttach()
    {
        base.OnAttach();
        // Load the font asset
        _font = AssetManager.LoadAsset<FontAsset>("Fonts/base");
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Owner == null || _font?.Font == null)
            return;

        Vector2 textSize = _font.MeasureStringVector(Text);
        Vector2 drawPosition = Owner.Position;

        // Apply alignment
        switch (Alignment)
        {
            case TextAlignment.Center:
                drawPosition.X -= textSize.X / 2;
                break;
            case TextAlignment.Right:
                drawPosition.X -= textSize.X;
                break;
        }

        spriteBatch.DrawString(_font.Font, Text, drawPosition, Color);
    }
}
```

Declared from XML (see `docs/XMLEntityDefinitions.md`):

```xml
<Component Type="CoreEssentials.Playground.Components.TextComponent">
  <Properties>
    <Property Name="Text" Value="Hello" />
    <Property Name="Color" Value="White" />
    <Property Name="Alignment" Value="Center" />
  </Properties>
</Component>
```

## XML-Based Asset Definitions

CoreEssentials uses XML files for defining complex assets:

### Sprite Sheet XML

```xml
<!-- Example sprite sheet XML (character_sheet.xml) -->
<SpriteSheetData xmlns="http://schemas.coreessentials.monogame/2025/spritesheet">
  <SourceType>texture2d</SourceType>
  <Source>character_malePerson_sheetHD</Source>
  <Grid>
    <Rows>5</Rows>
    <Columns>9</Columns>
  </Grid>
  <Origin>
    <X>96</X>
    <Y>128</Y>
  </Origin>
</SpriteSheetData>
```

### Sprite XML

```xml
<!-- Example sprite XML (character_sprite.xml) -->
<SpriteData xmlns="http://schemas.coreessentials.monogame/2025/sprite">
  <SourceType>spritesheet</SourceType>
  <Source>character_sheet.xml</Source>
  <Size>
    <Width>192</Width>
    <Height>256</Height>
  </Size>
  <Frame>0</Frame>
</SpriteData>
```

### Animated Sprite XML

```xml
<!-- Example animated sprite XML (character_anim_walk.xml) -->
<SpriteData xmlns="http://schemas.coreessentials.monogame/2025/sprite">
  <SourceType>spritesheet</SourceType>
  <Source>character_sheet.xml</Source>
  <Size>
    <Width>192</Width>
    <Height>256</Height>
  </Size>
  <Frames>36,37,38,39,40,41,42,43</Frames>
  <FrameRate>11</FrameRate>
</SpriteData>
```

### Audio Asset XML

```xml
<!-- Example sound effect XML (footstep1_sound.xml) -->
<SoundData xmlns="http://schemas.coreessentials.monogame/2025/audio">
  <Source>footstep00</Source>
  <SourceType>soundeffect</SourceType>
  <Volume>1</Volume>
</SoundData>
```

## Animation System

CoreEssentials includes a robust animation system built on the unified `Sprite` type and `AnimationState`:

```csharp
// Load an animated sprite from XML definition (the same Sprite type handles static and animated)
Sprite animatedSprite = AssetManager.LoadAsset<Sprite>("character_anim_walk.xml");

// Create an animation state to track the animation progress for an instance
AnimationState animState = new AnimationState(animatedSprite);

// Control animation playback
animState.IsPlaying = true; // Play the animation (default is true)
animState.IsLooping = true; // Loop the animation (default is true)
animState.Speed = 1.5f; // Speed up animation

// Update animation (call in Update method)
animState.Update(gameTime);

// Draw the current animation frame
animState.Draw(
    spriteBatch, 
    position, 
    Color.White,
    0f,
    SpriteEffects.None,
    0f
);

// You can also listen for animation completion
animState.AnimationCompleted += (sender, e) => {
    // Handle animation completion
};
```

## Asset Caching

The AssetManager caches assets and manages reference counting to prevent redundant loading:

```csharp
// First load - loads from file and caches
Sprite sprite1 = AssetManager.LoadAsset<Sprite>("character_sprite.xml");

// Second load - returns cached asset
Sprite sprite2 = AssetManager.LoadAsset<Sprite>("character_sprite.xml");

// sprite1 and sprite2 reference the same object

// The AssetManager keeps track of how many objects are using each asset
// When assets are no longer needed, they can be properly unloaded
// AssetManager.UnloadAsset(sprite1); // This decreases the reference count for the asset
```

## Example from Playground

The `CharacterScene` demonstrates asset usage. Characters are plain `GameObjectEntity` instances whose
sprites are loaded and rendered entirely by components (see [Character Components](CharacterComponents.md)) —
no per-entity C#. The built-in components declare their visuals with a string `SpriteAsset` property,
resolved through the `AssetManager` on attach:

```xml
<!-- A static, bouncing character: the SpriteComponent loads its own sprite and renders it. -->
<Component Type="SpriteComponent">
    <Properties>
        <Property Name="Origin" Value="0.5,0.5" />
        <Property Name="SpriteAsset" Value="Sprites/character_sprite.xml" />
    </Properties>
</Component>

<!-- An animated character: the walk animation loads the sprite and drives the AnimationComponent. -->
<Component Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn.AnimationComponent">
    <Properties>
        <Property Name="SpriteAsset" Value="Sprites/character_anim_walk.xml" />
        <Property Name="AnimationName" Value="walk" />
    </Properties>
</Component>
```

Both properties call `AssetManager.LoadAsset<Sprite>(...)` under the hood, so assets are cached and
reference-counted exactly as before. Because the AssetManager caches by name, a static sprite and an
animation referencing the same asset share one loaded instance.

## Best Practices

- Define assets through XML files following the proper schema namespaces
- Organize assets in a logical folder structure in the Content directory
- Always access assets through the static AssetManager.LoadAsset<T> method for automatic caching and reference counting
- Let animations play through AnimationState instances rather than manipulating the Sprite directly
- Make use of the Origin defined in sprite sheets for proper centering and rotation
- Use descriptive filenames for your XML asset definitions
- Consider memory usage when working with large textures
- Process your content assets through the MonoGame Content Pipeline
- Use AssetManager's reference counting to help manage memory efficiently