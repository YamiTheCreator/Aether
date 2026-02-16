using System.Runtime.CompilerServices;

namespace Aether.Core.Storage;

public sealed class ComponentPool<T> : IComponentPool
{
    public int Count { get; private set; }

    private readonly int[] _sparse;
    private int[] _entities;
    private T[] _dense;

    public ComponentPool( int maxEntities, int initialCapacity )
    {
        _sparse = new int[ maxEntities ];
        Array.Fill( _sparse, -1 );

        _entities = new int[ initialCapacity ];
        _dense = new T[ initialCapacity ];
        Count = 0;
    }

    public Span<T> Components => _dense.AsSpan( 0, Count );

    public ReadOnlySpan<int> Entities => _entities.AsSpan( 0, Count );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Has( int entityId )
    {
        return entityId < _sparse.Length &&
               _sparse[ entityId ] != -1 &&
               _sparse[ entityId ] < Count &&
               _entities[ _sparse[ entityId ] ] == entityId;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ref T Get( int entityId )
    {
        if ( !Has( entityId ) )
        {
            throw new InvalidOperationException( $"Entity {entityId} does not have component {typeof( T ).Name}" );
        }

        return ref _dense[ _sparse[ entityId ] ];
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Add( int entityId, in T component )
    {
        if ( Has( entityId ) )
        {
            _dense[ _sparse[ entityId ] ] = component;
            return;
        }

        if ( Count == _dense.Length )
        {
            Resize();
        }

        _sparse[ entityId ] = Count;
        _entities[ Count ] = entityId;
        _dense[ Count ] = component;
        Count++;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Remove( int entityId )
    {
        if ( !Has( entityId ) )
        {
            return;
        }

        int index = _sparse[ entityId ];
        int lastIndex = Count - 1;
        int lastEntityId = _entities[ lastIndex ];

        _dense[ index ] = _dense[ lastIndex ];
        _entities[ index ] = lastEntityId;
        _sparse[ lastEntityId ] = index;

        _sparse[ entityId ] = -1;
        _dense[ lastIndex ] = default!;
        Count--;
    }

    public void Clear()
    {
        Array.Fill( _sparse, -1 );
        Array.Clear( _dense, 0, Count );
        Count = 0;
    }

    private void Resize()
    {
        int newCapacity = _dense.Length * 2;
        Array.Resize( ref _dense, newCapacity );
        Array.Resize( ref _entities, newCapacity );
    }
}