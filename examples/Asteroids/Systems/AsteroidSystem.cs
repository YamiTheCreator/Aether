using Aether.Core;
using Asteroids.Components;
using Asteroids.Helpers;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Asteroids.Systems;

public class AsteroidSystem : SystemBase
{
    private readonly Random _random = new();
    private GL? _gl;

    protected override void OnCreate()
    {
        _gl = World.GetGlobal<GL>();

        for ( int i = 0; i < 5; i++ )
        {
            SpawnAsteroid( 3 );
        }
    }

    protected override void OnUpdate( float deltaTime )
    {
        GameStateSystem? gameStateSystem = World.GetSystem<GameStateSystem>();
        if ( gameStateSystem != null && gameStateSystem.IsGameOver() )
        {
            return; // Не обновляем астероиды если игра окончена
        }
        
        int asteroidCount = 0;
        foreach ( Entity entity in World.Filter<Asteroid>() )
        {
            asteroidCount++;
            ref Asteroid asteroid = ref World.Get<Asteroid>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            transform.Position.X += asteroid.Velocity.X * deltaTime;
            transform.Position.Y += asteroid.Velocity.Y * deltaTime;

            WrapPosition( ref transform.Position );
        }
        
        if ( asteroidCount == 0 && gameStateSystem != null && gameStateSystem.IsPlayerAlive() )
        {
            int newWaveCount = 5 + gameStateSystem.GetWaveNumber();
            for ( int i = 0; i < newWaveCount; i++ )
            {
                SpawnAsteroid( 3 );
            }
            gameStateSystem.NextWave();
        }
    }

    protected override void OnRender()
    {
        if ( _gl == null ) return;

        foreach ( Entity entity in World.Filter<Asteroid>() )
        {
            if ( !World.Has<Mesh>( entity ) )
            {
                Mesh mesh = CreateAsteroidMesh( _gl );
                World.Add( entity, mesh );
            }
        }
    }

    protected override void OnDestroy() { }

    public void SpawnAsteroid( int size, Vector3D<float>? position = null )
    {
        Vector3D<float> spawnPos = position ?? GetRandomEdgePosition();
        float scale = size switch
        {
            3 => 1.5f,
            2 => 1.0f,
            1 => 0.5f,
            _ => 1.0f
        };

        Vector2D<float> velocity = new(
            ( float )( _random.NextDouble() * 4 - 2 ),
            ( float )( _random.NextDouble() * 4 - 2 )
        );

        int seed = _random.Next();
        Vector2D<float>[] localVertices = CollisionHelper.CreateAsteroidVertices( seed );

        Asteroid asteroid = new()
        {
            Velocity = velocity,
            LocalVertices = localVertices,
            Seed = seed
        };

        Collider collider = new()
        {
            Type = ColliderType.Asteroid,
            LocalVertices = localVertices,
            Radius = 0.6f
        };

        Transform transform = new( spawnPos )
        {
            Scale = new Vector3D<float>( scale, scale, 1f ),
            Rotation = Quaternion<float>.Identity
        };

        Entity entity = World.Spawn( asteroid );
        World.Add( entity, transform );
        World.Add( entity, collider );
    }

    public void SplitAsteroid( Entity asteroidEntity )
    {
        if ( !World.Has<Asteroid>( asteroidEntity ) ) return;
        
        ref Transform transform = ref World.Get<Transform>( asteroidEntity );
        Vector3D<float> position = transform.Position;
        float currentScale = transform.Scale.X;

        World.Despawn( asteroidEntity );
        
        int newSize = currentScale switch
        {
            >= 1.5f => 2,
            >= 1.0f => 1,
            _ => 0
        };

        int points = currentScale switch
        {
            >= 1.5f => 20,
            >= 1.0f => 50,
            _ => 100
        };

        GameStateSystem? gameStateSystem = World.GetSystem<GameStateSystem>();
        gameStateSystem?.AddScore( points );
        
        if ( newSize > 0 )
        {
            for ( int i = 0; i < 2; i++ )
            {
                SpawnAsteroid( newSize, position );
            }
        }
    }

    private Vector3D<float> GetRandomEdgePosition()
    {
        const float worldWidth = 20f;
        const float worldHeight = 15f;

        int edge = _random.Next( 4 );
        return edge switch
        {
            0 => new Vector3D<float>( ( float )_random.NextDouble() * worldWidth - worldWidth / 2, worldHeight / 2, 0 ),
            1 => new Vector3D<float>( ( float )_random.NextDouble() * worldWidth - worldWidth / 2, -worldHeight / 2,
                0 ),
            2 => new Vector3D<float>( worldWidth / 2, ( float )_random.NextDouble() * worldHeight - worldHeight / 2,
                0 ),
            _ => new Vector3D<float>( -worldWidth / 2, ( float )_random.NextDouble() * worldHeight - worldHeight / 2,
                0 )
        };
    }

    private void WrapPosition( ref Vector3D<float> position )
    {
        const float worldWidth = 20f;
        const float worldHeight = 15f;

        if ( position.X > worldWidth / 2 ) position.X = -worldWidth / 2;
        if ( position.X < -worldWidth / 2 ) position.X = worldWidth / 2;
        if ( position.Y > worldHeight / 2 ) position.Y = -worldHeight / 2;
        if ( position.Y < -worldHeight / 2 ) position.Y = worldHeight / 2;
    }

    private static Mesh CreateAsteroidMesh( GL gl )
    {
        Random random = new();
        int sides = 8;
        Vertex[] vertices = new Vertex[ sides ];

        for ( int i = 0; i < sides; i++ )
        {
            float angle = ( float )i / sides * MathF.PI * 2;
            float radius = 0.4f + ( float )random.NextDouble() * 0.2f;

            float x = MathF.Cos( angle ) * radius;
            float y = MathF.Sin( angle ) * radius;

            vertices[ i ] = new Vertex(
                new Vector3D<float>( x, y, 0f ),
                Vector2D<float>.Zero,
                new Vector4D<float>( 1, 1, 1, 1 ),
                0,
                Vector3D<float>.UnitZ
            );
        }

        uint[] indices = new uint[ sides + 1 ];
        for ( uint i = 0; i < sides; i++ )
        {
            indices[ i ] = i;
        }

        indices[ sides ] = 0;

        Mesh mesh = MeshSystem.CreateMeshFromVertices( gl, vertices, indices );
        mesh.Topology = PrimitiveType.LineStrip;

        Material material = new()
        {
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f )
        };
        mesh.Material = material;

        return mesh;
    }
}