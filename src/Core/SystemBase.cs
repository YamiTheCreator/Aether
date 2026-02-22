namespace Aether.Core;

public abstract class SystemBase
{
    protected World World { get; private set; } = null!;

    protected virtual void OnCreate() { }
    
    protected virtual void OnUpdate( float deltaTime ) { }
    
    protected virtual void OnRender() { }
    
    protected virtual void OnDestroy() { }
    
    internal void Create( World world )
    {
        World = world;
        OnCreate();
    }

    internal void Update( World world, float deltaTime )
    {
        World = world;
        OnUpdate( deltaTime );
    }

    internal void Render( World world )
    {
        World = world;
        OnRender();
    }

    internal void Destroy( World world )
    {
        World = world;
        OnDestroy();
    }
}