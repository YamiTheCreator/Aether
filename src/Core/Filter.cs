namespace Aether.Core;

public readonly struct Filter<T> where T : Component
{
    private readonly World _world;
    private readonly ComponentPoolImpl<T> _poolImpl;

    internal Filter( World world, ComponentPoolImpl<T> poolImpl )
    {
        _world = world;
        _poolImpl = poolImpl;
    }

    public Enumerator GetEnumerator() => new( _world, _poolImpl );

    public ref struct Enumerator
    {
        private readonly World _world;
        private readonly ComponentPoolImpl<T> _poolImpl;
        private readonly ReadOnlySpan<int> _entities;
        private int _index;

        internal Enumerator( World world, ComponentPoolImpl<T> poolImpl )
        {
            _world = world;
            _poolImpl = poolImpl;
            _entities = poolImpl.Entities;
            _index = -1;
        }

        public Entity Current => _world.GetEntity( _entities[ _index ] );

        public bool MoveNext()
        {
            _index++;
            return _index < _poolImpl.Count;
        }
    }

    public FilterWith<T, TU> With<TU>() where TU : Component
    {
        return new FilterWith<T, TU>( _world, _poolImpl );
    }
}

public readonly struct FilterWith<T, TU>
    where T : Component
    where TU : Component
{
    private readonly World _world;
    private readonly ComponentPoolImpl<T> _poolImplT;

    internal FilterWith( World world, ComponentPoolImpl<T> poolImplT )
    {
        _world = world;
        _poolImplT = poolImplT;
    }

    public Enumerator GetEnumerator() => new( _world, _poolImplT );

    public ref struct Enumerator
    {
        private readonly World _world;
        private readonly ComponentPoolImpl<T> _poolImplT;
        private readonly ComponentPoolImpl<TU>? _poolU;
        private readonly ReadOnlySpan<int> _entities;
        private int _index;

        internal Enumerator( World world, ComponentPoolImpl<T> poolImplT )
        {
            _world = world;
            _poolImplT = poolImplT;
            _poolU = world.TryGetPool<TU>();
            _entities = poolImplT.Entities;
            _index = -1;
        }

        public Entity Current => _world.GetEntity( _entities[ _index ] );

        public bool MoveNext()
        {
            if ( _poolU == null ) return false;

            while ( ++_index < _poolImplT.Count )
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