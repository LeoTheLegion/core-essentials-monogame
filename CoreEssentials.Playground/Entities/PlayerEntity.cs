using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.Inputs;
using CoreEssentials.Timing; // Added for Time.DeltaTime

namespace CoreEssentials.Playground.Entities;

public class PlayerEntity : CharacterEntity
{
    private const float MoveSpeed = 1f; // Speed in units per millisecond

    public PlayerEntity(Vector2 position) : base(position)
    {
    }

    // Parameterless constructor for XML-based entity loading (Scene-as-Data, Sprint 5d).
    public PlayerEntity() : base(Vector2.Zero)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Use the new Input.Keyboard for polling
        if (Input.Keyboard.IsKeyDown(Keys.Left))
        {
            Position += new Vector2(-1, 0) * MoveSpeed * (float)Time.DeltaTime;
        }
        if (Input.Keyboard.IsKeyDown(Keys.Right))
        {
            Position += new Vector2(1, 0) * MoveSpeed * (float)Time.DeltaTime;
        }
        if (Input.Keyboard.IsKeyDown(Keys.Up))
        {
            Position += new Vector2(0, -1) * MoveSpeed * (float)Time.DeltaTime;
        }
        if (Input.Keyboard.IsKeyDown(Keys.Down))
        {
            Position += new Vector2(0, 1) * MoveSpeed * (float)Time.DeltaTime;
        }
    }
}
