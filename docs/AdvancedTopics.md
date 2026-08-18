# Advanced Topics and Best Practices

This guide covers advanced topics and best practices when working with the CoreEssentials-MonoGame framework.

## Performance Optimization

### Entity Management

- **Entity Pooling**: Reuse entities instead of creating/destroying them frequently:

```csharp
public class BulletPool
{
    private List<BulletEntity> _activePool = new List<BulletEntity>();
    private Queue<BulletEntity> _inactivePool = new Queue<BulletEntity>();
    private EntitySystem _entitySystem;
    private Scene _scene;
    
    public BulletPool(Scene scene, int initialSize)
    {
        _scene = scene;
        _entitySystem = scene.GetGameSystem<EntitySystem>();
        
        // Pre-create bullets
        for (int i = 0; i < initialSize; i++)
        {
            BulletEntity bullet = _entitySystem.CreateEntity<BulletEntity>(Vector2.Zero);
            bullet.SetActive(false);
            _inactivePool.Enqueue(bullet);
        }
    }
    
    public BulletEntity GetBullet()
    {
        BulletEntity bullet;
        
        if (_inactivePool.Count > 0)
        {
            bullet = _inactivePool.Dequeue();
        }
        else
        {
            // Create new if pool is empty
            bullet = _entitySystem.CreateEntity<BulletEntity>(Vector2.Zero);
        }
        
        bullet.SetActive(true);
        _activePool.Add(bullet);
        return bullet;
    }
    
    public void ReturnBullet(BulletEntity bullet)
    {
        bullet.SetActive(false);
        _activePool.Remove(bullet);
        _inactivePool.Enqueue(bullet);
    }
}
```

### Physics Optimization

- **Use appropriate collision categories**: Limit what can collide with what
- **Sleep bodies**: Allow physics bodies to sleep when inactive
- **Use compound shapes**: Use multiple simple shapes instead of complex ones
- **Limit physics bodies**: Only use physics for entities that need it

```csharp
using CoreEssentials.GameSystems.Physics.Types;

// Set up collision categories on a collider
ICollider collider = playerBody.CreateCircleCollider(radius);
collider.Categories   = CollisionCategory.Cat1;
collider.CollidesWith = CollisionCategory.Cat2 | CollisionCategory.Cat3; // Only collide with categories 2 and 3

// Create a compound shape: attach multiple colliders to one body
IPhysicsBody body = physics.CreateDynamic(position);
body.CreateCircleCollider(radius1, offset: new Vector2(0, -10));
body.CreateRectangleCollider(new Vector2(width, height), offset: Vector2.Zero);
body.CreateCircleCollider(radius2, offset: new Vector2(0, 10));
```

> **Tip:** the recommended way to add physics to entities is via components — `RigidbodyComponent` (the body) plus one or more `ColliderComponent` (the shapes). See [Collision Groups & Filtering](./CollisionGroups.md) for the full filtering model.

## Scene Management Patterns

### Scene Transitions

Implement smooth transitions between scenes:

```csharp
public class TransitionManager
{
    private static Texture2D _fadeTexture;
    private static float _alpha = 0f;
    private static bool _isFading = false;
    private static bool _isFadingIn = false;
    private static float _fadeSpeed = 1f;
    private static Scene _nextScene;
    
    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _fadeTexture = new Texture2D(graphicsDevice, 1, 1);
        _fadeTexture.SetData(new[] { Color.Black });
    }
    
    public static void Update(GameTime gameTime)
    {
        if (!_isFading) return;
        
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (_isFadingIn)
        {
            _alpha -= dt * _fadeSpeed;
            if (_alpha <= 0f)
            {
                _alpha = 0f;
                _isFading = false;
            }
        }
        else
        {
            _alpha += dt * _fadeSpeed;
            if (_alpha >= 1f)
            {
                _alpha = 1f;
                _isFadingIn = true;
                
                // Load the next scene
                if (_nextScene != null)
                {
                    SceneManager.LoadScene(_nextScene);
                    _nextScene = null;
                }
            }
        }
    }
    
    public static void Draw(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_alpha <= 0f) return;
        
        spriteBatch.Begin();
        spriteBatch.Draw(_fadeTexture, viewport.Bounds, Color.Black * _alpha);
        spriteBatch.End();
    }
    
    public static void FadeToScene(Scene nextScene, float speed = 1f)
    {
        _nextScene = nextScene;
        _fadeSpeed = speed;
        _isFading = true;
        _isFadingIn = false;
        _alpha = 0f;
    }
}
```

### Scene Persistence

Keep data between scenes:

```csharp
public static class GameState
{
    // Data to persist between scenes
    public static int Score { get; set; }
    public static int HighScore { get; set; }
    public static int Lives { get; set; }
    public static int Level { get; set; }
    public static Dictionary<string, object> CustomData = new Dictionary<string, object>();
    
    public static void Reset()
    {
        Score = 0;
        Lives = 3;
        Level = 1;
        CustomData.Clear();
    }
    
    public static void SaveHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            // Optionally save to disk
        }
    }
}
```

## Advanced Input Handling

### Action-Based Input System

Create an action-based input system for better abstraction:

```csharp
public class InputAction
{
    private List<Func<bool>> _conditions = new List<Func<bool>>();
    private bool _wasTriggeredLastFrame = false;
    
    public event Action<InputActionEventArgs> Performed;
    public event Action<InputActionEventArgs> Started;
    public event Action<InputActionEventArgs> Canceled;
    
    public string Name { get; private set; }
    
    public InputAction(string name)
    {
        Name = name;
    }
    
    public InputAction AddKeyBinding(Keys key)
    {
        _conditions.Add(() => Input.Keyboard.IsKeyDown(key));
        return this;
    }
    
    public InputAction AddMouseBinding(MouseButton button)
    {
        _conditions.Add(() => Input.Mouse.IsButtonDown(button));
        return this;
    }
    
    public InputAction AddGamepadBinding(PlayerIndex playerIndex, Buttons button)
    {
        _conditions.Add(() => Input.Gamepad.IsButtonDown(playerIndex, button));
        return this;
    }
    
    public void Update()
    {
        bool isTriggered = false;
        foreach (var condition in _conditions)
        {
            if (condition())
            {
                isTriggered = true;
                break;
            }
        }
        
        if (isTriggered && !_wasTriggeredLastFrame)
        {
            Started?.Invoke(new InputActionEventArgs(this));
        }
        
        if (isTriggered)
        {
            Performed?.Invoke(new InputActionEventArgs(this));
        }
        
        if (!isTriggered && _wasTriggeredLastFrame)
        {
            Canceled?.Invoke(new InputActionEventArgs(this));
        }
        
        _wasTriggeredLastFrame = isTriggered;
    }
}

public class InputActionMap
{
    private Dictionary<string, InputAction> _actions = new Dictionary<string, InputAction>();
    
    public InputAction CreateAction(string name)
    {
        var action = new InputAction(name);
        _actions[name] = action;
        return action;
    }
    
    public InputAction GetAction(string name)
    {
        if (_actions.TryGetValue(name, out var action))
        {
            return action;
        }
        return null;
    }
    
    public void Update()
    {
        foreach (var action in _actions.Values)
        {
            action.Update();
        }
    }
}
```

Usage:

```csharp
// Setup
var inputMap = new InputActionMap();

var jumpAction = inputMap.CreateAction("Jump")
    .AddKeyBinding(Keys.Space)
    .AddGamepadBinding(PlayerIndex.One, Buttons.A);

var moveAction = inputMap.CreateAction("Move")
    .AddKeyBinding(Keys.Right)
    .AddKeyBinding(Keys.D)
    .AddGamepadBinding(PlayerIndex.One, Buttons.DPadRight);

jumpAction.Started += OnJumpStarted;

// In Update method
inputMap.Update();

// Handler
private void OnJumpStarted(InputActionEventArgs args)
{
    player.Jump();
}
```

## Advanced Physics Integration

### Contact Listeners

Set up advanced collision detection. The recommended approach is the per-body `OnCollision` / `OnSeparation` events rather than touching the engine's contact manager directly:

```csharp
// On the player's physics body
IPhysicsBody playerBody = ...;

playerBody.OnCollision += args =>
{
    IPhysicsBody other = args.BodyB == playerBody ? args.BodyA : args.BodyB;

    // Resolve the owning entity from the body's type tag
    if (other.Type == "Player" && TryGetEntity(other, out Entity entity))
    {
        if (entity is EnemyEntity enemy)
            ((PlayerEntity)entity).OnHitEnemy(enemy);
    }

    return true; // Allow the collision (return false to reject)
};

playerBody.OnSeparation += args =>
{
    // Handle end of contact
};
```

See [Physics System — Collision Events](./PhysicsSystem.md) for the full event model, including per-collider granularity.

### Ragdoll Physics

A ragdoll is a set of dynamic bodies connected by joints. Each body part can be its own entity with a `RigidbodyComponent`, or a single entity with multiple colliders. Note that joint types (`IRevoluteJoint`, `IWeldJoint`, `IDistanceJoint`) are currently **internal-use** in the abstraction layer — they exist for the engine but are not yet exposed as a public creation API. Until they are, build multi-part bodies from several `IPhysicsBody` instances and keep their relative positions in your own update logic:

```csharp
public class RagdollEntity : Entity
{
    private IPhysicsBody _torso;
    private IPhysicsBody _head;
    
    public override void OnStart()
    {
        base.OnStart();
        
        PhysicsEngine physics = EntitySystem.GetGameSystem<PhysicsEngine>();
        
        // Create torso
        _torso = physics.CreateDynamic(Position);
        _torso.CreateRectangleCollider(new Vector2(40, 60));
        
        // Create head
        _head = physics.CreateDynamic(Position + new Vector2(0, -40));
        _head.CreateCircleCollider(radius: 20);
        
        // ...more body parts...
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Follow the torso
        if (_torso != null)
        {
            Position = _torso.Position;
            Rotation = _torso.Rotation;
        }
    }
}
```

## Memory Management

### Asset Unloading

Implement proper asset management to avoid memory leaks:

```csharp
using CoreEssentials.Assets;

public class GameplayScene : Scene
{
    protected override IEnumerator OnStartCoroutine()
    {
        // Load assets through the static AssetManager (reference-counted)
        var playerSprite = AssetManager.LoadAsset<Sprite>("player_texture.png");
        var enemySprite  = AssetManager.LoadAsset<Sprite>("enemy_texture.png");
        var background   = AssetManager.LoadAsset<Sprite>("level_background.png");
        var explosion    = AssetManager.LoadAsset<AudioClip>("explosion_sound.wav");
        
        yield return null;
        
        // Scene setup...
    }
    
    public override void Unload()
    {
        base.Unload();
        
        // Release scene-specific assets (decrements the reference count)
        AssetManager.UnloadAsset<Sprite>("player_texture.png");
        AssetManager.UnloadAsset<Sprite>("enemy_texture.png");
        AssetManager.UnloadAsset<Sprite>("level_background.png");
        AssetManager.UnloadAsset<AudioClip>("explosion_sound.wav");
    }
}
```

## AI Integration

Implement basic AI behavior using state machines:

```csharp
public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Retreat
}

public class EnemyEntity : Entity
{
    private EnemyState _currentState = EnemyState.Idle;
    private StateMachine<EnemyState> _stateMachine;
    private Entity _target;
    private RigidbodyComponent _rigidbody;
    private float _detectionRange = 200f;
    private float _attackRange = 50f;
    
    public override void OnStart()
    {
        base.OnStart();
        
        // Set up physics body via component
        _rigidbody = new RigidbodyComponent(RigidbodyType.Dynamic);
        AddComponent(_rigidbody);
        AddComponent(new ColliderComponent(radius: 16f));
        
        // Create state machine
        _stateMachine = new StateMachine<EnemyState>();
        
        // Configure states
        _stateMachine.Configure(EnemyState.Idle)
            .OnEnter(EnterIdleState)
            .OnUpdate(UpdateIdleState)
            .Permit(EnemyState.Patrol, () => true);
            
        _stateMachine.Configure(EnemyState.Patrol)
            .OnEnter(EnterPatrolState)
            .OnUpdate(UpdatePatrolState)
            .Permit(EnemyState.Chase, () => CanSeeTarget());
            
        _stateMachine.Configure(EnemyState.Chase)
            .OnEnter(EnterChaseState)
            .OnUpdate(UpdateChaseState)
            .Permit(EnemyState.Attack, () => IsTargetInAttackRange())
            .Permit(EnemyState.Patrol, () => !CanSeeTarget());
            
        _stateMachine.Configure(EnemyState.Attack)
            .OnEnter(EnterAttackState)
            .OnUpdate(UpdateAttackState)
            .Permit(EnemyState.Chase, () => !IsTargetInAttackRange() && CanSeeTarget())
            .Permit(EnemyState.Retreat, () => _health < _retreatThreshold);
            
        // Start in idle state
        _stateMachine.Enter(EnemyState.Idle);
        
        // Find player target
        _target = FindTarget();
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update state machine
        _stateMachine.Update(gameTime);
        
        // Position/Rotation are synced from the physics body automatically.
    }
    
    private Entity FindTarget()
    {
        // Find player entity (EntitySystem is the protected field on Entity)
        return EntitySystem?.FindByType<PlayerEntity>().FirstOrDefault();
    }
    
    private bool CanSeeTarget()
    {
        if (_target == null) return false;
        
        float distance = Vector2.Distance(Position, _target.Position);
        return distance <= _detectionRange;
    }
    
    private bool IsTargetInAttackRange()
    {
        if (_target == null) return false;
        
        float distance = Vector2.Distance(Position, _target.Position);
        return distance <= _attackRange;
    }
    
    // State behaviors
    private void EnterIdleState() { /* Implementation */ }
    private void UpdateIdleState(GameTime gameTime) { /* Implementation */ }
    private void EnterPatrolState() { /* Implementation */ }
    private void UpdatePatrolState(GameTime gameTime) { /* Implementation */ }
    private void EnterChaseState() { /* Implementation */ }
    private void UpdateChaseState(GameTime gameTime)
    {
        if (_target != null)
        {
            // Move toward target
            Vector2 direction = _target.Position - Position;
            direction.Normalize();
            _rigidbody.ApplyImpulse(direction * 10f);
        }
    }
    private void EnterAttackState() { /* Implementation */ }
    private void UpdateAttackState(GameTime gameTime) { /* Implementation */ }
}

// Simple state machine implementation
public class StateMachine<TState> where TState : struct, IConvertible
{
    private TState _currentState;
    private Dictionary<TState, StateConfig<TState>> _configurations = new Dictionary<TState, StateConfig<TState>>();
    
    public void Configure(TState state)
    {
        if (!_configurations.ContainsKey(state))
        {
            _configurations[state] = new StateConfig<TState>(state);
        }
        return _configurations[state];
    }
    
    public void Enter(TState state)
    {
        if (_configurations.TryGetValue(state, out var config))
        {
            _currentState = state;
            config.OnEnterAction?.Invoke();
        }
    }
    
    public void Update(GameTime gameTime)
    {
        if (_configurations.TryGetValue(_currentState, out var config))
        {
            config.OnUpdateAction?.Invoke(gameTime);
            
            // Check for transitions
            foreach (var transition in config.Transitions)
            {
                if (transition.Value.Invoke())
                {
                    Enter(transition.Key);
                    break;
                }
            }
        }
    }
}

public class StateConfig<TState> where TState : struct, IConvertible
{
    public TState State { get; private set; }
    public Action OnEnterAction { get; private set; }
    public Action<GameTime> OnUpdateAction { get; private set; }
    public Dictionary<TState, Func<bool>> Transitions { get; private set; } = new Dictionary<TState, Func<bool>>();
    
    public StateConfig(TState state)
    {
        State = state;
    }
    
    public StateConfig<TState> OnEnter(Action action)
    {
        OnEnterAction = action;
        return this;
    }
    
    public StateConfig<TState> OnUpdate(Action<GameTime> action)
    {
        OnUpdateAction = action;
        return this;
    }
    
    public StateConfig<TState> Permit(TState nextState, Func<bool> condition)
    {
        Transitions[nextState] = condition;
        return this;
    }
}
```

## Conclusion

These advanced techniques will help you create more sophisticated games with the CoreEssentials-MonoGame framework. Combine these patterns with the framework's built-in systems to create efficient, maintainable game code.