namespace Aether.Core;

public abstract class ComponentBase : IComponent
{
    protected virtual void OnAdd( Entity entity, World world )
    {
    }

    protected virtual void OnRemove( Entity entity, World world )
    {
    }

    internal void InternalOnAdd( Entity entity, World world )
    {
        OnAdd( entity, world );
    }

    internal void InternalOnRemove( Entity entity, World world )
    {
        OnRemove( entity, world );
    }
}