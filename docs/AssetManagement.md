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

// Load an animated sprite
AnimatedSprite animSprite = AssetManager.LoadAsset<AnimatedSprite>("character_anim_walk.xml");

// Load audio
SoundAsset sound = AssetManager.LoadAsset<SoundAsset>("footstep1_sound.xml");

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

// Sprite sheets are typically used by Sprite and AnimatedSprite classes
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

The `TextEntity` class in the playground demonstrates how to use FontAsset with different alignment options:

```csharp
public class TextEntity : Entity
{
    private FontAsset _font;
    private string _text;
    private Color _color;
    private TextAlignment _alignment;
    
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }
    
    public override void OnStart()
    {
        base.OnStart();
        
        // Load the font asset
        _font = AssetManager.LoadAsset<FontAsset>("base");
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);
        
        if (_font == null || _font.Font == null)
            return;
            
        Vector2 textSize = _font.MeasureStringVector(_text);
        Vector2 drawPosition = _position;
        
        // Apply alignment
        switch (_alignment)
        {
            case TextAlignment.Center:
                drawPosition.X -= textSize.X / 2;
                break;
            case TextAlignment.Right:
                drawPosition.X -= textSize.X;
                break;
        }
        
        spriteBatch.DrawString(_font.Font, _text, drawPosition, _color);
    }
}
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
<AnimatedSpriteData xmlns="http://schemas.coreessentials.monogame/2025/sprite">
  <SourceType>spritesheet</SourceType>
  <Source>character_sheet.xml</Source>
  <Size>
    <Width>192</Width>
    <Height>256</Height>
  </Size>
  <Frames>36,37,38,39,40,41,42,43</Frames>
  <FrameRate>11</FrameRate>
</AnimatedSpriteData>
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

CoreEssentials includes a robust animation system with `AnimatedSprite` and `AnimationState`:

```csharp
// Load an animated sprite from XML definition
AnimatedSprite animatedSprite = AssetManager.LoadAsset<AnimatedSprite>("character_anim_walk.xml");

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

The `CharacterScene` demonstrates asset usage:

```csharp
public class CharacterEntity : Entity
{
    private Sprite _sprite;
    
    public CharacterEntity(Vector2 position)
    {
        _position = position;
        
        // Load the character sprite that references the sprite sheet
        _sprite = AssetManager.LoadAsset<Sprite>("character_sprite.xml");
    }
    
    public override void OnStart()
    {
        base.OnStart();
        Console.WriteLine("Character entity created!");
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        // Draw the character with the current frame
        _sprite.Draw(
            spriteBatch, 
            _position, 
            Color.White, 
            0f, 
            SpriteEffects.None, 
            0f
        );
    }
}

public class AnimatedCharacterEntity : Entity
{
    private AnimatedSprite _animatedSprite;
    private AnimationState _animationState;
    
    public AnimatedCharacterEntity(Vector2 position)
    {
        _position = position;
        
        // Load the animated sprite
        _animatedSprite = (AnimatedSprite)AssetManager.LoadAsset<AnimatedSprite>("character_anim_walk.xml");
        
        // Create animation state for this instance
        _animationState = new AnimationState(_animatedSprite);
    }
    
    public override void OnStart()
    {
        base.OnStart();
        Console.WriteLine("Animated character entity created!");
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update the animation
        _animationState.Update(gameTime);
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        SpriteEffects effects = SpriteEffects.None;
        
        // Draw the animated character using the current animation state
        _animationState.Draw(
            spriteBatch, 
            _position, 
            Color.White,
            0f,
            effects,
            0f
        );
    }
}
```

## Best Practices

- Define assets through XML files following the proper schema namespaces
- Organize assets in a logical folder structure in the Content directory
- Always access assets through the static AssetManager.LoadAsset<T> method for automatic caching and reference counting
- Let animations play through AnimationState instances rather than manipulating the AnimatedSprite directly
- Make use of the Origin defined in sprite sheets for proper centering and rotation
- Use descriptive filenames for your XML asset definitions
- Consider memory usage when working with large textures
- Process your content assets through the MonoGame Content Pipeline
- Use AssetManager's reference counting to help manage memory efficiently