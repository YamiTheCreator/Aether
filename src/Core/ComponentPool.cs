namespace Aether.Core;

public interface ComponentPool
{
    bool Has( int entityId );

    void Remove( int entityId );

    void Clear();
}