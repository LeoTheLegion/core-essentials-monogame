# Coroutines

The Coroutine system in CoreEssentials-MonoGame provides a powerful way to manage time-based operations and asynchronous sequences without blocking the main game loop. Coroutines allow you to write code that spans multiple frames in a sequential manner.

## Key Components

### IEnumerator-based Coroutines

Coroutines are implemented using C#'s `IEnumerator` interface, allowing you to pause and resume execution.

```csharp
// Example of a coroutine method
private IEnumerator MyCoroutine()
{
    // Do something on the first frame
    Console.WriteLine("Starting coroutine");
    
    // Wait for 2 seconds
    yield return new WaitForSeconds(2.0f);
    
    // Do something after the wait
    Console.WriteLine("2 seconds have passed");
    
    // Wait until a condition is met
    yield return new WaitUntil(() => Input.Keyboard.IsKeyDown(Keys.Space));
    
    // Do something after the condition is met
    Console.WriteLine("Space key was pressed");
}
```

### Yield Instructions

The framework provides several yield instructions to control coroutine flow:

- **WaitForSeconds**: Pauses the coroutine for a specified duration
- **WaitUntil**: Pauses the coroutine until a condition is met
- **null**: Yields for one frame

```csharp
// Wait for 1 second
yield return new WaitForSeconds(1.0f);

// Wait until a condition is met
yield return new WaitUntil(() => someCondition);

// Wait for one frame
yield return null;
```

### Starting Coroutines

Coroutines can be started from different parts of the framework:

```csharp
// In a Scene-derived class
StartCoroutine(MyCoroutine());

// In other classes with a reference to a coroutine owner
coroutineOwner.StartCoroutine(MyCoroutine());
```

## Practical Applications

### Loading Sequences

Coroutines are ideal for implementing loading sequences with progress updates:

```csharp
protected override IEnumerator OnStartCoroutine()
{
    UpdateLoadingProgress(0.0f, "Starting...");
    yield return null;
    
    UpdateLoadingProgress(0.3f, "Loading resources...");
    yield return new WaitForSeconds(0.5f);
    
    UpdateLoadingProgress(0.6f, "Setting up game...");
    yield return null;
    
    UpdateLoadingProgress(1.0f, "Ready!");
}
```

### Timed Events

Use coroutines for events that need to happen over time:

```csharp
private IEnumerator FadeInEffect(float duration)
{
    float elapsed = 0;
    
    while (elapsed < duration)
    {
        float alpha = elapsed / duration;
        SetAlpha(alpha);
        
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    SetAlpha(1.0f);
}
```

### Nested Coroutines

Coroutines can be nested to create complex sequences:

```csharp
private IEnumerator MainSequence()
{
    yield return StartCoroutine(SubSequence1());
    yield return StartCoroutine(SubSequence2());
    Console.WriteLine("All sequences complete");
}

private IEnumerator SubSequence1()
{
    // Sequence 1 logic
    yield return new WaitForSeconds(1.0f);
}

private IEnumerator SubSequence2()
{
    // Sequence 2 logic
    yield return new WaitForSeconds(1.0f);
}
```

## Example from Playground

The `PhysicsEntityScene` demonstrates coroutines for creating entities with progress updates:

```csharp
protected override IEnumerator OnStartCoroutine()
{
    UpdateLoadingProgress(0.5f, "Initializing physics scene...");
    yield return new WaitForSeconds(0.2f);
    
    // Create entities one by one with progress updates
    for (int i = 0; i < totalEntities; i++)
    {
        // Create entity
        Ball ball = entitySystem.CreateEntity<Ball>(new Vector2(i * 10, y));
        
        // Update progress
        float progress = 0.55f + 0.35f * (i / (float)totalEntities);
        UpdateLoadingProgress(progress, $"Creating entities: {i}/{totalEntities} balls");
        
        if (i % 50 == 0)
            yield return null; // Yield occasionally to update the UI
    }
    
    UpdateLoadingProgress(1.0f, "Scene ready!");
}
```

## Best Practices

- Use coroutines for operations that need to span multiple frames
- Avoid long-running loops without yields to prevent freezing
- Clean up running coroutines when objects are destroyed
- Use appropriate yield instructions to control timing
- Utilize nested coroutines for complex sequences