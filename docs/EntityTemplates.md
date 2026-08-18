# Entity Templates

Entity templates provide reusable blueprints for creating entities. Define entities in XML and instantiate them multiple times with different parameters.

## Overview

Templates provide:
- Reusable entity definitions
- XML-based configuration
- Fast instantiation
- Consistent entity creation
- Hierarchical templates

## API Reference

### RegisterTemplate(string name, string assetName)

Registers a template from an XML asset.

```csharp
public void RegisterTemplate(string name, string assetName)
```

**Parameters:**
- `name` — Unique template name
- `assetName` — XML asset name

**Example:**
```csharp
entitySystem.RegisterTemplate("EnemyGoblin", "templates/enemy_goblin.xml");
entitySystem.RegisterTemplate("Player", "templates/player.xml");
```

### Instantiate(string templateName, Vector2 position)

Instantiates an entity from a registered template.

```csharp
public Entity Instantiate(string templateName, Vector2 position)
```

**Parameters:**
- `templateName` — Registered template name
- `position` — World position

**Returns:** New entity instance

**Example:**
```csharp
var enemy = entitySystem.Instantiate("EnemyGoblin", new Vector2(100, 200));
```

## Template XML Schema

### Basic Template

```xml
<EntityTemplate Type="EnemyEntity" Rotation="0" Sort="0" Active="true">
    <Tags>
        <Tag Name="Enemy" />
        <Tag Name="GroundUnit" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Color" Value="255,255,255,255" />
                <Property Name="LayerDepth" Value="0.5" />
            </Properties>
        </Component>
        <Component Type="RigidbodyComponent">
            <Properties>
                <Property Name="Mass" Value="1.0" />
                <Property Name="FixedRotation" Value="false" />
                <Property Name="SyncFromPhysics" Value="true" />
            </Properties>
        </Component>
    </Components>
</EntityTemplate>
```

> **Note on `RigidbodyComponent` in templates:** The rigidbody *type* (`Static`, `Dynamic`,
> `Kinematic`) is chosen via the constructor — `new RigidbodyComponent(RigidbodyType.Dynamic)` —
> and is **not** a settable XML property. `RigidbodyComponent` has no parameterless constructor,
> so a template cannot create it on its own: add it in your entity's `OnStart()` first, and the
> template will then apply the settable properties (`Mass`, `FixedRotation`, `SyncFromPhysics`)
> via reflection.

### Attributes

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `Type` | string | Yes | Entity class name |
| `Rotation` | float | No | Initial rotation (radians) |
| `Sort` | int | No | Render sort order |
| `Active` | bool | No | Active by default |

### Tags Element

```xml
<Tags>
    <Tag Name="Enemy" />
    <Tag Name="GroundUnit" />
</Tags>
```

### Components Element

```xml
<Components>
    <Component Type="SpriteComponent">
        <Properties>
            <Property Name="Color" Value="255,255,255,255" />
            <Property Name="LayerDepth" Value="0.5" />
        </Properties>
    </Component>
</Components>
```

> **Note:** The `Sprite` itself is a complex asset and is assigned in code
> (e.g. `new SpriteComponent(AssetManager.LoadAsset<Sprite>("goblin_sprite.xml"))`),
> not via a string XML property. Templates set the simple, settable properties
> such as `Color`, `Origin`, `Effects`, and `LayerDepth`.

### Children Element

```xml
<EntityTemplate Type="Spaceship">
    <Children>
        <EntityTemplate Type="Engine">
            <Position X="-50" Y="0" />
        </EntityTemplate>
    </Children>
</EntityTemplate>
```

## Usage Examples

### Basic Template Usage

```csharp
// Register template
entitySystem.RegisterTemplate("EnemyGoblin", "templates/enemy_goblin.xml");

// Instantiate multiple enemies
for (int i = 0; i < 10; i++)
{
    var position = new Vector2(
        Random.Range(0, 800),
        Random.Range(0, 600)
    );
    
    var enemy = entitySystem.Instantiate("EnemyGoblin", position);
}
```

### Enemy Waves

```csharp
public class WaveSpawner
{
    public void SpawnWave(int count, string templateName)
    {
        for (int i = 0; i < count; i++)
        {
            var position = GetSpawnPosition();
            var enemy = entitySystem.Instantiate(templateName, position);
            
            // Customize instance
            enemy.SetTag($"Wave_{currentWave}");
        }
    }
}
```

### Template with Overrides

```csharp
public class TemplateManager
{
    public Entity CreateCustomEnemy(string templateName, Vector2 position, 
        string[] additionalTags, float scale)
    {
        var enemy = entitySystem.Instantiate(templateName, position);
        
        // Add additional tags
        foreach (var tag in additionalTags)
        {
            enemy.SetTag(tag);
        }
        
        // Modify the entity's scale (SpriteComponent has no Scale property)
        enemy.Scale = new Vector2(scale, scale);
        
        return enemy;
    }
}
```

### Hierarchical Templates

```xml
<EntityTemplate Type="Spaceship" Rotation="0" Sort="0">
    <Tags>
        <Tag Name="Vehicle" />
        <Tag Name="Player" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Color" Value="255,255,255,255" />
                <Property Name="LayerDepth" Value="1.0" />
            </Properties>
        </Component>
    </Components>
    <Children>
        <EntityTemplate Type="Engine">
            <Position X="-50" Y="0" />
            <Components>
                <Component Type="SpriteComponent">
                    <Properties>
                        <Property Name="Color" Value="128,128,128,255" />
                    </Properties>
                </Component>
            </Components>
        </EntityTemplate>
        <EntityTemplate Type="Engine">
            <Position X="50" Y="0" />
            <Components>
                <Component Type="SpriteComponent">
                    <Properties>
                        <Property Name="Color" Value="128,128,128,255" />
                    </Properties>
                </Component>
            </Components>
        </EntityTemplate>
    </Children>
</EntityTemplate>
```

```csharp
// Instantiate spaceship with engines
var ship = entitySystem.Instantiate("Spaceship", new Vector2(400, 300));
// Engines automatically created as children
```

## Template Loading

### From Asset

```csharp
// Load from XML asset
entitySystem.RegisterTemplate("Enemy", "templates/enemy.xml");
```

### From File (Testing)

```csharp
// Load from file for testing
var template = EntityTemplateLoader.LoadFromFile("templates/enemy.xml");
```

### From XML String

```csharp
var xml = @"
<EntityTemplate Type='EnemyEntity'>
    <Tags>
        <Tag Name='Enemy' />
    </Tags>
</EntityTemplate>";

var template = EntityTemplateLoader.LoadFromXml(xml);
```

## Advanced Usage

### Template Variations

```csharp
public class EnemyFactory
{
    public Entity CreateEnemy(string type, Vector2 position)
    {
        string templateName = type switch
        {
            "Goblin" => "EnemyGoblin",
            "Orc" => "EnemyOrc",
            "Troll" => "EnemyTroll",
            _ => "EnemyGoblin"
        };
        
        var enemy = entitySystem.Instantiate(templateName, position);
        
        // Apply type-specific modifications
        ApplyTypeModifiers(enemy, type);
        
        return enemy;
    }
    
    private void ApplyTypeModifiers(Entity enemy, string type)
    {
        switch (type)
        {
            case "Goblin":
                enemy.SetTag("Fast");
                break;
            case "Orc":
                enemy.SetTag("Strong");
                break;
            case "Troll":
                enemy.SetTag("Tank");
                break;
        }
    }
}
```

### Dynamic Template Registration

```csharp
public void LoadTemplatesFromDirectory(string directory)
{
    var files = Directory.GetFiles(directory, "*.xml");
    
    foreach (var file in files)
    {
        var templateName = Path.GetFileNameWithoutExtension(file);
        var assetName = $"templates/{templateName}";
        
        entitySystem.RegisterTemplate(templateName, assetName);
    }
}
```

### Template Inheritance

```xml
<!-- Base enemy template -->
<EntityTemplate Type="EnemyEntity" Id="BaseEnemy">
    <Tags>
        <Tag Name="Enemy" />
    </Tags>
    <Components>
        <Component Type="HealthComponent">
            <Properties>
                <Property Name="MaxHealth" Value="100" />
            </Properties>
        </Component>
    </Components>
</EntityTemplate>

<!-- Specific enemy -->
<EntityTemplate Type="GoblinEntity">
    <Tags>
        <Tag Name="Goblin" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Color" Value="255,255,255,255" />
                <Property Name="LayerDepth" Value="0.5" />
            </Properties>
        </Component>
    </Components>
</EntityTemplate>
```

## Best Practices

### Template Organization

**Do:**
- Organize templates by type
- Use descriptive names
- Keep templates focused
- Document template parameters

**Don't:**
- Create overly complex templates
- Duplicate template definitions
- Hardcode positions in templates
- Mix logic with templates

### Performance

- Register templates once at startup
- Reuse templates for multiple instances
- Pre-load templates for critical entities
- Cache template references

### Template Design

**Good template:**
```xml
<EntityTemplate Type="EnemyEntity">
    <Tags>
        <Tag Name="Enemy" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Color" Value="255,255,255,255" />
            </Properties>
        </Component>
    </Components>
</EntityTemplate>
```

**Bad template:**
```xml
<EntityTemplate Type="EnemyEntity">
    <!-- Too specific -->
    <Position X="100" Y="200" />
    <!-- Hardcoded values -->
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="LayerDepth" Value="1.5" />
                <Property Name="Color" Value="255,0,0,255" />
                <Property Name="Origin" Value="0.5,0.5" />
                <!-- Too many properties -->
            </Properties>
        </Component>
    </Components>
</EntityTemplate>
```

## See Also

- [XML Entity Definitions](./XMLEntityDefinitions.md) — Loading entities from XML
- [Entity System](./EntitySystem.md) — Core entity management
- [Entity Hierarchy](./EntityHierarchy.md) — Parent-child relationships
