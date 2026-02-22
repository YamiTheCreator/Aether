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

public readonly struct Filter<T1, T2>
    where T1 : Component
    where T2 : Component
{
    private readonly World _world;
    private readonly ComponentPoolImpl<T1> _poolImpl1;

    internal Filter( World world, ComponentPoolImpl<T1> poolImpl1 )
    {
        _world = world;
        _poolImpl1 = poolImpl1;
    }

    public Enumerator GetEnumerator() => new( _world, _poolImpl1 );

    public ref struct Enumerator
    {
        private readonly World _world;
        private readonly ComponentPoolImpl<T1> _poolImpl1;
        private readonly ComponentPoolImpl<T2>? _poolImpl2;
        private readonly ReadOnlySpan<int> _entities;
        private int _index;

        internal Enumerator( World world, ComponentPoolImpl<T1> poolImpl1 )
        {
            _world = world;
            _poolImpl1 = poolImpl1;
            _poolImpl2 = world.TryGetPool<T2>();
            _entities = poolImpl1.Entities;
            _index = -1;
        }

        public Entity Current => _world.GetEntity( _entities[ _index ] );

        public bool MoveNext()
        {
            if ( _poolImpl2 == null ) return false;

            while ( ++_index < _poolImpl1.Count )
            {
                int entityId = _entities[ _index ];
                if ( _poolImpl2.Has( entityId ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}

public readonly struct Filter<T1, T2, T3>
    where T1 : Component
    where T2 : Component
    where T3 : Component
{
    private readonly World _world;
    private readonly ComponentPoolImpl<T1> _poolImpl1;

    internal Filter( World world, ComponentPoolImpl<T1> poolImpl1 )
    {
        _world = world;
        _poolImpl1 = poolImpl1;
    }

    public Enumerator GetEnumerator() => new( _world, _poolImpl1 );

    public ref struct Enumerator
    {
        private readonly World _world;
        private readonly ComponentPoolImpl<T1> _poolImpl1;
        private readonly ComponentPoolImpl<T2>? _poolImpl2;
        private readonly ComponentPoolImpl<T3>? _poolImpl3;
        private readonly ReadOnlySpan<int> _entities;
        private int _index;

        internal Enumerator( World world, ComponentPoolImpl<T1> poolImpl1 )
        {
            _world = world;
            _poolImpl1 = poolImpl1;
            _poolImpl2 = world.TryGetPool<T2>();
            _poolImpl3 = world.TryGetPool<T3>();
            _entities = poolImpl1.Entities;
            _index = -1;
        }

        public Entity Current => _world.GetEntity( _entities[ _index ] );

        public bool MoveNext()
        {
            if ( _poolImpl2 == null || _poolImpl3 == null ) return false;

            while ( ++_index < _poolImpl1.Count )
            {
                int entityId = _entities[ _index ];
                if ( _poolImpl2.Has( entityId ) && _poolImpl3.Has( entityId ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}

public readonly struct Filter<T1, T2, T3, T4>
    where T1 : Component
    where T2 : Component
    where T3 : Component
    where T4 : Component
{
    private readonly World _world;
    private readonly ComponentPoolImpl<T1> _poolImpl1;

    internal Filter( World world, ComponentPoolImpl<T1> poolImpl1 )
    {
        _world = world;
        _poolImpl1 = poolImpl1;
    }

    public Enumerator GetEnumerator() => new( _world, _poolImpl1 );

    public ref struct Enumerator
    {
        private readonly World _world;
        private readonly ComponentPoolImpl<T1> _poolImpl1;
        private readonly ComponentPoolImpl<T2>? _poolImpl2;
        private readonly ComponentPoolImpl<T3>? _poolImpl3;
        private readonly ComponentPoolImpl<T4>? _poolImpl4;
        private readonly ReadOnlySpan<int> _entities;
        private int _index;

        internal Enumerator( World world, ComponentPoolImpl<T1> poolImpl1 )
        {
            _world = world;
            _poolImpl1 = poolImpl1;
            _poolImpl2 = world.TryGetPool<T2>();
            _poolImpl3 = world.TryGetPool<T3>();
            _poolImpl4 = world.TryGetPool<T4>();
            _entities = poolImpl1.Entities;
            _index = -1;
        }

        public Entity Current => _world.GetEntity( _entities[ _index ] );

        public bool MoveNext()
        {
            if ( _poolImpl2 == null || _poolImpl3 == null || _poolImpl4 == null ) return false;

            while ( ++_index < _poolImpl1.Count )
            {
                int entityId = _entities[ _index ];
                if ( _poolImpl2.Has( entityId ) && _poolImpl3.Has( entityId ) && _poolImpl4.Has( entityId ) )
                {
                    return true;
                }
            }

            return false;
        }
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