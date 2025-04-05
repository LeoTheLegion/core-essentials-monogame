
using CoreEssentials.GameSystems.EntitySystems.EntityOOPsystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

public abstract class Entity
{
    protected Vector2 _position;
    protected int sort = -1;
    protected bool _active;

    protected Entity()
    {
        _active = true;
        EntityManagementSystem.Register(this);
    }

    public abstract void LoadAssets();
    public abstract void Update(ref GameTime gameTime);
    public abstract void Render(ref SpriteBatch _spriteBatch);
    public virtual void Destroy()
    {
        EntityManagementSystem.Unregister(this);
    }

    public virtual Entity SetSort(int x)
    {
        sort = x;
        return this;
    }

    public virtual int GetSort() { return sort; }
    public virtual void SetActive(bool active) => _active = active;
    public virtual bool GetActive() { return _active; }
}
