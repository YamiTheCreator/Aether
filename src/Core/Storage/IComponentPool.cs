namespace Aether.Core.Storage;

public interface IComponentPool
{
    bool Has( int entityId );

    void Remove( int entityId );

    void Clear();
}