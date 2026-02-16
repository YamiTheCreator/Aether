namespace Aether.Core.Systems;

public abstract class SystemBase
{
    protected World World { get; private set; } = null!;

    protected virtual void OnInit() { }
    
    protected virtual void OnUpdate( float deltaTime ) { }
    
    protected virtual void OnRender() { }
    
    protected virtual void OnCleanup() { }
    
    internal void Init( World world )
    {
        World = world;
        OnInit();
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

    internal void Cleanup( World world )
    {
        World = world;
        OnCleanup();
    }
}