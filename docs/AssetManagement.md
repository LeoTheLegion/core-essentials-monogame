# Asset Management

The Asset Management system in CoreEssentials-MonoGame simplifies the loading, caching, and use of various game assets such as textures, sprites, audio, fonts, and custom XML data.

## Key Components

### AssetManager

The `AssetManager` class is the central component for asset loading and management:

```csharp
// Access the asset manager (typically available through the MainGame instance)
AssetManager assetManager = SceneManager.Game.AssetManager;

// Load a texture
Texture2D texture = assetManager.LoadTexture("character_sprite.png");

// Load a sprite sheet
SpriteSheet spriteSheet = assetManager.LoadSpriteSheet("character_sheet.xml");

// Load audio
SoundEffect soundEffect = assetManager.LoadSoundEffect("explosion.wav");

// Load a font
FontAsset font = assetManager.LoadAsset<FontAsset>("base");
```

## Sprite Management

CoreEssentials provides robust sprite and animation support:

### Sprite

The `Sprite` class represents a single image or a part of a texture:

```csharp
// Create a sprite from a texture
Sprite sprite = new Sprite(texture);

// Create a sprite from a region of a texture
Sprite sprite = new Sprite(texture, new Rectangle(0, 0, 32, 32));

// Set sprite properties
sprite.Origin = new Vector2(16, 16); // Set origin to center
sprite.Color = Color.White * 0.8f;  // Apply tinting/transparency
sprite.Scale = new Vector2(2, 2);   // Scale the sprite
sprite.Rotation = MathHelper.ToRadians(45); // Rotate 45 degrees

// Draw the sprite
sprite.Draw(spriteBatch, position);
```

### SpriteSheet

The `SpriteSheet` class manages sprite atlases and animations:

```csharp
// Load a sprite sheet from XML definition
SpriteSheet sheet = assetManager.LoadSpriteSheet("character_sheet.xml");

// Get a specific sprite from the sheet by name
Sprite idleSprite = sheet.GetSprite("character_idle");

// Get an animation by name
Animation walkAnimation = sheet.GetAnimation("character_walk");
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
<SpriteSheet>
  <Texture>character_malePerson_sheetHD.png</Texture>
  <Sprites>
    <Sprite name="idle" x="0" y="0" width="64" height="128" />
    <Sprite name="walk1" x="64" y="0" width="64" height="128" />
    <Sprite name="walk2" x="128" y="0" width="64" height="128" />
    <Sprite name="jump" x="192" y="0" width="64" height="128" />
  </Sprites>
  <Animations>
    <Animation name="walk" fps="8" loop="true">
      <Frame sprite="walk1" />
      <Frame sprite="walk2" />
    </Animation>
    <Animation name="idle_anim" fps="4" loop="true">
      <Frame sprite="idle" />
    </Animation>
  </Animations>
</SpriteSheet>
```

### Audio Asset XML

```xml
<!-- Example sound effect XML (footstep_sound.xml) -->
<SoundEffect>
  <File>footstep01.ogg</File>
  <Volume>0.8</Volume>
  <Pitch>0.0</Pitch>
  <Pan>0.0</Pan>
</SoundEffect>
```

## Animation System

CoreEssentials includes a robust animation system:

```csharp
// Load an animation from a sprite sheet
Animation walkAnimation = spriteSheet.GetAnimation("walk");

// Create an animation player
AnimationPlayer animPlayer = new AnimationPlayer(walkAnimation);

// Control animation playback
animPlayer.Play();
animPlayer.Pause();
animPlayer.Stop();
animPlayer.IsLooping = true;
animPlayer.PlaybackSpeed = 1.5f; // Speed up animation

// Update animation (call in Update method)
animPlayer.Update(gameTime);

// Get the current sprite from the animation
Sprite currentSprite = animPlayer.CurrentSprite;

// Draw the current animation frame
currentSprite.Draw(spriteBatch, position);
```

## Asset Caching

The AssetManager caches assets to prevent redundant loading:

```csharp
// First load - loads from file and caches
Texture2D texture1 = assetManager.LoadTexture("character.png");

// Second load - returns cached asset
Texture2D texture2 = assetManager.LoadTexture("character.png");

// texture1 and texture2 reference the same object
```

## Example from Playground

The `CharacterScene` demonstrates asset usage:

```csharp
public class CharacterEntity : Entity
{
    protected Sprite _sprite;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Load character sprite from XML definition
        SpriteSheet sheet = Scene.AssetManager.LoadSpriteSheet("character_sprite.xml");
        _sprite = sheet.GetSprite("default");
        
        // Center the sprite origin
        _sprite.Origin = new Vector2(_sprite.Width / 2, _sprite.Height / 2);
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        
        // Draw the sprite at the entity position
        _sprite.Draw(spriteBatch, Position);
    }
}

public class AnimatedCharacterEntity : Entity
{
    protected Animation _walkAnimation;
    protected AnimationPlayer _animPlayer;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Load animation from XML definition
        SpriteSheet sheet = Scene.AssetManager.LoadSpriteSheet("character_anim_walk.xml");
        _walkAnimation = sheet.GetAnimation("walk");
        
        // Create and play animation
        _animPlayer = new AnimationPlayer(_walkAnimation);
        _animPlayer.Play();
        _animPlayer.IsLooping = true;
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update animation
        _animPlayer.Update(gameTime);
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        
        // Draw the current animation frame
        Sprite currentSprite = _animPlayer.CurrentSprite;
        currentSprite.Draw(spriteBatch, Position);
    }
}
```

## Best Practices

- Use XML files to define complex assets like sprite sheets and animations
- Organize assets in a logical folder structure
- Access assets through the AssetManager for automatic caching
- Unload assets when scenes change if memory usage is a concern
- Define reusable assets in shared XML files
- Set sprite origins appropriately for rotation and positioning
- Use clear, descriptive names for sprites and animations
- Consider memory usage when working with large textures
- Use asset preprocessing to optimize at build time