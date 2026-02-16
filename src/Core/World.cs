using Aether.Core.Options;
using Aether.Core.Querying;
using Aether.Core.Storage;
using Aether.Core.Systems;

namespace Aether.Core;

public sealed class World : IDisposable
{
    private readonly Dictionary<Type, IComponentPool> _pools = new();
    private readonly Dictionary<Type, object> _globals = new();
    private readonly List<SystemBase> _systems = [];

    private readonly Queue<int> _recycledIds = new();
    private readonly int[] _versions;
    private int _nextEntityId;

    private readonly BaseOptions _options;

    public World( BaseOptions? options = null )
    {
        _options = options ?? new BaseOptions();
        _versions = new int[ _options.MaxEntities ];
    }

    public int EntityCount => _nextEntityId - _recycledIds.Count;

    public void SetGlobal<T>( T value ) where T : notnull
    {
        _globals[ typeof( T ) ] = value;
    }

    public T GetGlobal<T>() where T : notnull
    {
        if ( !_globals.TryGetValue( typeof( T ), out object? value ) )
        {
            throw new KeyNotFoundException( $"Global resource of type {typeof( T ).Name} not found." );
        }

        return ( T )value;
    }

    public bool HasGlobal<T>() => _globals.ContainsKey( typeof( T ) );

    public void AddSystem( SystemBase system )
    {
        _systems.Add( system );
    }

    public void Init()
    {
        foreach ( SystemBase system in _systems )
        {
            system.Init( this );
        }
    }

    public void Update( float deltaTime )
    {
        foreach ( SystemBase system in _systems )
        {
            system.Update( this, deltaTime );
        }
    }

    public void Render()
    {
        foreach ( SystemBase system in _systems )
        {
            system.Render( this );
        }
    }

    public void Cleanup()
    {
        foreach ( SystemBase system in _systems )
        {
            system.Cleanup( this );
        }
    }

    public Entity Spawn()
    {
        Entity entity = AllocateEntity();
        return entity;
    }

    public Entity Spawn<T>( in T component ) where T : IComponent
    {
        Entity entity = AllocateEntity();
        Add( entity, component );
        return entity;
    }

    public Entity Spawn<T1, T2>( in T1 c1, in T2 c2 )
        where T1 : IComponent
        where T2 : IComponent
    {
        Entity entity = AllocateEntity();
        Add( entity, c1 );
        Add( entity, c2 );
        return entity;
    }

    public Entity Spawn<T1, T2, T3>( in T1 c1, in T2 c2, in T3 c3 )
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        Entity entity = AllocateEntity();
        Add( entity, c1 );
        Add( entity, c2 );
        Add( entity, c3 );
        return entity;
    }

    public Entity Spawn<T1, T2, T3, T4>( in T1 c1, in T2 c2, in T3 c3, in T4 c4 )
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        Entity entity = AllocateEntity();
        Add( entity, c1 );
        Add( entity, c2 );
        Add( entity, c3 );
        Add( entity, c4 );
        return entity;
    }

    public void Despawn( Entity entity )
    {
        if ( !IsAlive( entity ) ) return;
        _versions[ entity.Id ]++;
        foreach ( IComponentPool pool in _pools.Values ) pool.Remove( entity.Id );
        _recycledIds.Enqueue( entity.Id );
    }

    public bool IsAlive( Entity entity )
    {
        return entity.Id >= 0 &&
               entity.Id < _nextEntityId &&
               _versions[ entity.Id ] == entity.Version;
    }

    public Entity GetEntity( int id )
    {
        return new Entity( id, _versions[ id ] );
    }

    private Entity AllocateEntity()
    {
        int id;
        if ( _recycledIds.TryDequeue( out int recycledId ) )
        {
            id = recycledId;
        }
        else
        {
            if ( _nextEntityId >= _options.MaxEntities )
                throw new InvalidOperationException( "Max entities limit reached." );
            id = _nextEntityId++;
            _versions[ id ] = 0;
        }

        return new Entity( id, _versions[ id ] );
    }

    public void Add<T>( Entity entity, in T component ) where T : IComponent
    {
        if ( !IsAlive( entity ) ) throw new InvalidOperationException( "Entity is not alive." );
        GetPool<T>().Add( entity.Id, component );

        if ( component is ComponentBase componentBase )
        {
            componentBase.InternalOnAdd( entity, this );
        }
    }

    public void Remove<T>( Entity entity ) where T : IComponent
    {
        if ( !IsAlive( entity ) ) throw new InvalidOperationException( "Entity is not alive." );

        ref T component = ref GetPool<T>().Get( entity.Id );
        if ( component is ComponentBase componentBase )
        {
            componentBase.InternalOnRemove( entity, this );
        }

        GetPool<T>().Remove( entity.Id );
    }

    public ref T Get<T>( Entity entity ) where T : IComponent
    {
        if ( !IsAlive( entity ) ) throw new InvalidOperationException( "Entity is not alive." );
        return ref GetPool<T>().Get( entity.Id );
    }

    public bool Has<T>( Entity entity ) where T : IComponent
    {
        if ( !IsAlive( entity ) ) return false;
        if ( _pools.TryGetValue( typeof( T ), out IComponentPool? pool ) ) return pool.Has( entity.Id );
        return false;
    }

    public Filter<T> Filter<T>() where T : IComponent => new( this, GetPool<T>() );

    public ComponentPool<T> GetPool<T>() where T : IComponent
    {
        Type type = typeof( T );
        if ( !_pools.TryGetValue( type, out IComponentPool? pool ) )
        {
            pool = new ComponentPool<T>( _options.MaxEntities, _options.InitialCapacity );
            _pools[ type ] = pool;
        }

        return ( ComponentPool<T> )pool;
    }

    internal ComponentPool<T>? TryGetPool<T>() where T : IComponent
    {
        _pools.TryGetValue( typeof( T ), out IComponentPool? pool );
        return ( ComponentPool<T>? )pool;
    }

    public void Dispose()
    {
        Cleanup();
        foreach ( IComponentPool pool in _pools.Values ) pool.Clear();
        _pools.Clear();
        _recycledIds.Clear();
        _systems.Clear();
        _globals.Clear();
    }
}