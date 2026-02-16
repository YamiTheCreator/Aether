using Aether.Core.Storage;

namespace Aether.Core.Querying;

public readonly struct Filter<T> where T : IComponent
{
    private readonly World _world;
    private readonly ComponentPool<T> _pool;

    internal Filter( World world, ComponentPool<T> pool )
    {
        _world = world;
        _pool = pool;
    }

    public Enumerator GetEnumerator() => new( _world, _pool );

    public ref struct Enumerator
    {
        private readonly World _world;
        private readonly ComponentPool<T> _pool;
        private readonly ReadOnlySpan<int> _entities;
        private int _index;

        internal Enumerator( World world, ComponentPool<T> pool )
        {
            _world = world;
            _pool = pool;
            _entities = pool.Entities;
            _index = -1;
        }

        public Entity Current => _world.GetEntity( _entities[ _index ] );

        public bool MoveNext()
        {
            _index++;
            return _index < _pool.Count;
        }
    }

    public FilterWith<T, TU> With<TU>() where TU : IComponent
    {
        return new FilterWith<T, TU>( _world, _pool );
    }
}

public readonly struct FilterWith<T, TU>
    where T : IComponent
    where TU : IComponent
{
    private readonly World _world;
    private readonly ComponentPool<T> _poolT;

    internal FilterWith( World world, ComponentPool<T> poolT )
    {
        _world = world;
        _poolT = poolT;
    }

    public Enumerator GetEnumerator() => new( _world, _poolT );

    public ref struct Enumerator
    {
        private readonly World _world;
        private readonly ComponentPool<T> _poolT;
        private readonly ComponentPool<TU>? _poolU;
        private readonly ReadOnlySpan<int> _entities;
        private int _index;

        internal Enumerator( World world, ComponentPool<T> poolT )
        {
            _world = world;
            _poolT = poolT;
            _poolU = world.TryGetPool<TU>();
            _entities = poolT.Entities;
            _index = -1;
        }

        public Entity Current => _world.GetEntity( _entities[ _index ] );

        public bool MoveNext()
        {
            if ( _poolU == null ) return false;

            while ( ++_index < _poolT.Count )
            {
                int entityId = _entities[ _index ];
                if ( _poolU.Has( entityId ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}