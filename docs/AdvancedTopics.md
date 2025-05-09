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
// Set up collision categories
Body playerBody = physics.CreateCircle(position, radius, 1f);
playerBody.CollisionCategories = Category.Cat1;
playerBody.CollidesWith = Category.Cat2 | Category.Cat3; // Only collide with categories 2 and 3

// Allow body to sleep when not moving
playerBody.SleepingAllowed = true;

// Create compound shape
Body compoundBody = physics.World.CreateBody();
compoundBody.BodyType = BodyType.Dynamic;
compoundBody.CreateFixture(physics.World.CreateCircleShape(radius1, 1f, new Vector2(0, -10)));
compoundBody.CreateFixture(physics.World.CreateRectangleShape(width, height, 1f, Vector2.Zero));
compoundBody.CreateFixture(physics.World.CreateCircleShape(radius2, 1f, new Vector2(0, 10)));
```

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

Set up advanced collision detection:

```csharp
public class CollisionManager : GameSystem
{
    private PhysicsEngine _physicsEngine;
    
    public override void Initialize()
    {
        base.Initialize();
        
        _physicsEngine = Scene.GetGameSystem<PhysicsEngine>();
        _physicsEngine.World.ContactManager.BeginContact += OnBeginContact;
        _physicsEngine.World.ContactManager.EndContact += OnEndContact;
    }
    
    private void OnBeginContact(Contact contact)
    {
        // Get the entities from the fixtures' user data
        var entityA = contact.FixtureA.Body.Tag as Entity;
        var entityB = contact.FixtureB.Body.Tag as Entity;
        
        if (entityA is PlayerEntity && entityB is EnemyEntity)
        {
            (entityA as PlayerEntity).OnHitEnemy(entityB as EnemyEntity);
        }
        else if (entityA is EnemyEntity && entityB is PlayerEntity)
        {
            (entityB as PlayerEntity).OnHitEnemy(entityA as EnemyEntity);
        }
    }
    
    private void OnEndContact(Contact contact)
    {
        // Handle end of contact
    }
    
    public override void Dispose()
    {
        if (_physicsEngine != null && _physicsEngine.World != null)
        {
            _physicsEngine.World.ContactManager.BeginContact -= OnBeginContact;
            _physicsEngine.World.ContactManager.EndContact -= OnEndContact;
        }
        
        base.Dispose();
    }
}
```

### Ragdoll Physics

Implement advanced physics effects like ragdoll:

```csharp
public class RagdollEntity : Entity
{
    private List<Body> _bodyParts = new List<Body>();
    private List<Joint> _joints = new List<Joint>();
    
    public override void Initialize()
    {
        base.Initialize();
        
        PhysicsEngine physics = Scene.GetGameSystem<PhysicsEngine>();
        
        // Create torso
        Body torso = physics.CreateRectangle(Position, 40, 60, 1f);
        torso.BodyType = BodyType.Dynamic;
        _bodyParts.Add(torso);
        
        // Create head
        Body head = physics.CreateCircle(Position + new Vector2(0, -40), 20, 1f);
        head.BodyType = BodyType.Dynamic;
        _bodyParts.Add(head);
        
        // Create limbs
        Body leftArm = physics.CreateRectangle(Position + new Vector2(-30, 0), 30, 10, 1f);
        leftArm.BodyType = BodyType.Dynamic;
        _bodyParts.Add(leftArm);
        
        // ...more body parts...
        
        // Connect with joints
        var neckJoint = physics.World.CreateRevoluteJoint(torso, head, 
            head.Position + new Vector2(0, 20),
            new Vector2(0, -30));
        neckJoint.LowerLimit = -0.5f;
        neckJoint.UpperLimit = 0.5f;
        neckJoint.LimitEnabled = true;
        _joints.Add(neckJoint);
        
        // ...more joints...
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update entity position to follow the torso
        if (_bodyParts.Count > 0)
        {
            Position = _bodyParts[0].Position;
            Rotation = _bodyParts[0].Rotation;
        }
    }
}
```

## Memory Management

### Asset Unloading

Implement proper asset management to avoid memory leaks:

```csharp
public class GameplayScene : Scene
{
    private List<string> _sceneAssets = new List<string>();
    
    protected override IEnumerator OnStartCoroutine()
    {
        // Track loaded assets
        _sceneAssets.Add("player_texture.png");
        _sceneAssets.Add("enemy_texture.png");
        _sceneAssets.Add("level_background.png");
        _sceneAssets.Add("explosion_sound.wav");
        
        // Load assets
        AssetManager assetManager = Game.AssetManager;
        assetManager.LoadTexture(_sceneAssets[0]);
        assetManager.LoadTexture(_sceneAssets[1]);
        assetManager.LoadTexture(_sceneAssets[2]);
        assetManager.LoadSoundEffect(_sceneAssets[3]);
        
        yield return null;
        
        // Scene setup...
    }
    
    public override void Unload()
    {
        base.Unload();
        
        // Unload scene-specific assets
        AssetManager assetManager = Game.AssetManager;
        foreach (string asset in _sceneAssets)
        {
            assetManager.UnloadAsset(asset);
        }
        
        // Force garbage collection if needed
        GC.Collect();
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
    private Body _body;
    private float _detectionRange = 200f;
    private float _attackRange = 50f;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Set up physics body
        PhysicsEngine physics = Scene.GetGameSystem<PhysicsEngine>();
        _body = physics.CreateCircle(Position, 16f, 1f);
        _body.BodyType = BodyType.Dynamic;
        
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
        
        // Update position from physics body
        Position = _body.Position;
        Rotation = _body.Rotation;
    }
    
    private Entity FindTarget()
    {
        // Find player entity
        EntitySystem entitySystem = Scene.GetGameSystem<EntitySystem>();
        return entitySystem.GetEntitiesOfType<PlayerEntity>().FirstOrDefault();
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
            _body.ApplyForce(direction * 10f);
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