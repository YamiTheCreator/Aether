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

        foreach ( Entity entity in World.Filter<Asteroid, Collider>() )
        {
            if ( !World.Has<Mesh>( entity ) )
            {
                ref Collider collider = ref World.Get<Collider>( entity );
                Mesh mesh = CreateAsteroidMesh( _gl, collider.LocalVertices );
                World.Add( entity, mesh );
            }
        }
    }

    protected override void OnDestroy() { }

    public void SpawnAsteroid( int size, Vector3D<float>? position = null )
    {
        Vector3D<float> spawnPos = position ?? GetRandomPosition();
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
        Vector2D<float>[] localVertices = VerticesBuilder.CreateAsteroidVertices( seed );

        Asteroid asteroid = new()
        {
            Velocity = velocity,
        };

        Collider collider = new()
        {
            LocalVertices = localVertices
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

    private Vector3D<float> GetRandomPosition()
    {
        WorldBounds bounds = GetWorldBounds();
        int edge = _random.Next( 4 );
        return edge switch
        {
            0 => new Vector3D<float>( ( float )_random.NextDouble() * bounds.Width - bounds.Width / 2,
                bounds.Height / 2, 0 ),
            1 => new Vector3D<float>( ( float )_random.NextDouble() * bounds.Width - bounds.Width / 2,
                -bounds.Height / 2, 0 ),
            2 => new Vector3D<float>( bounds.Width / 2,
                ( float )_random.NextDouble() * bounds.Height - bounds.Height / 2, 0 ),
            _ => new Vector3D<float>( -bounds.Width / 2,
                ( float )_random.NextDouble() * bounds.Height - bounds.Height / 2, 0 )
        };
    }

    private WorldBounds GetWorldBounds()
    {
        foreach ( Entity entity in World.Filter<WorldBounds>() )
        {
            return World.Get<WorldBounds>( entity );
        }

        return WorldBounds.Default;
    }

    private void WrapPosition( ref Vector3D<float> position )
    {
        WorldBounds bounds = GetWorldBounds();
        if ( position.X > bounds.Width / 2 ) position.X = -bounds.Width / 2;
        if ( position.X < -bounds.Width / 2 ) position.X = bounds.Width / 2;
        if ( position.Y > bounds.Height / 2 ) position.Y = -bounds.Height / 2;
        if ( position.Y < -bounds.Height / 2 ) position.Y = bounds.Height / 2;
    }

    private static Mesh CreateAsteroidMesh( GL gl, Vector2D<float>[] localVertices )
    {
        Vertex[] vertices = ConvertToVertices( localVertices );
        uint[] indices = CreateLineStripIndices( localVertices.Length );

        Mesh mesh = MeshSystem.CreateMeshFromVertices( gl, vertices, indices );
        mesh.Topology = PrimitiveType.LineStrip;
        mesh.Material = CreateWhiteMaterial();

        return mesh;
    }

    private static Vertex[] ConvertToVertices( Vector2D<float>[] localVertices )
    {
        Vertex[] vertices = new Vertex[ localVertices.Length ];

        for ( int i = 0; i < localVertices.Length; i++ )
        {
            vertices[ i ] = new Vertex(
                new Vector3D<float>( localVertices[ i ].X, localVertices[ i ].Y, 0f ),
                Vector2D<float>.Zero,
                new Vector4D<float>( 1, 1, 1, 1 ),
                0,
                Vector3D<float>.UnitZ
            );
        }

        return vertices;
    }

    private static uint[] CreateLineStripIndices( int vertexCount )
    {
        uint[] indices = new uint[ vertexCount + 1 ];
        for ( uint i = 0; i < vertexCount; i++ )
        {
            indices[ i ] = i;
        }
        indices[ vertexCount ] = 0;

        return indices;
    }

    private static Material CreateWhiteMaterial()
    {
        return new Material
        {
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f )
        };
    }
}