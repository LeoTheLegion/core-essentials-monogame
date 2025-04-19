using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

public abstract class Entity
{
    protected Vector2 _position;
    protected int sort = -1;
    protected bool _destroyed;
    protected bool _active;

    private bool _hasStarted = false;

    public bool Destroyed => _destroyed;
    public bool HasStarted => _hasStarted;

    protected EntitySystem EntitySystem;

    protected Entity()
    {
        _position = Vector2.Zero;
        sort = -1;
        _destroyed = false;
        _active = true;
    }

    public void SetGameSystem(EntitySystem entitySystem)
    {
        EntitySystem = entitySystem;
    }

    public virtual void OnStart(){
        _hasStarted = true;
    }
    public virtual void Update(GameTime gameTime) { }
    public virtual void Render(SpriteBatch _spriteBatch) { }
    public virtual void Destroy()
    {
        _destroyed = true;
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
