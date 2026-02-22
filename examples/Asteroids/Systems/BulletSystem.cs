using Aether.Core;
using Asteroids.Components;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Asteroids.Systems;

public class BulletSystem : SystemBase
{
    private GL? _gl;

    protected override void OnCreate()
    {
        _gl = World.GetGlobal<GL>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        GameStateSystem? gameStateSystem = World.GetSystem<GameStateSystem>();
        if ( gameStateSystem != null && gameStateSystem.IsGameOver() )
        {
            return; // Не обновляем пули если игра окончена
        }

        List<Entity> toRemove = [ ];
        WorldBounds bounds = GetWorldBounds();

        foreach ( Entity entity in World.Filter<Bullet>() )
        {
            ref Bullet bullet = ref World.Get<Bullet>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            transform.Position.X += bullet.Velocity.X * deltaTime;
            transform.Position.Y += bullet.Velocity.Y * deltaTime;

            WrapPosition( ref transform.Position, bounds );

            bullet.Lifetime -= deltaTime;

            if ( bullet.Lifetime <= 0 )
            {
                toRemove.Add( entity );
            }
        }

        foreach ( Entity entity in toRemove )
        {
            World.Despawn( entity );
        }
    }

    protected override void OnRender()
    {
        if ( _gl == null ) return;

        foreach ( Entity entity in World.Filter<Bullet>() )
        {
            if ( !World.Has<Mesh>( entity ) )
            {
                Mesh mesh = CreateBulletMesh( _gl );
                World.Add( entity, mesh );
            }
        }
    }

    protected override void OnDestroy() { }

    public void SpawnBullet( Vector3D<float> position, Vector2D<float> direction )
    {
        BulletConfig config = GetBulletConfig();

        Vector3D<float> spawnPosition = position;
        spawnPosition.X += direction.X * 0.5f;
        spawnPosition.Y += direction.Y * 0.5f;

        Bullet bullet = new()
        {
            Velocity = direction * config.Speed,
            Lifetime = config.DefaultLifetime
        };

        Vector2D<float>[] bulletVertices =
        [
            new Vector2D<float>( -0.15f, -0.15f ),
            new Vector2D<float>( 0.15f, -0.15f ),
            new Vector2D<float>( 0.15f, 0.15f ),
            new Vector2D<float>( -0.15f, 0.15f )
        ];

        Collider collider = new()
        {
            Type = ColliderType.Bullet,
            LocalVertices = bulletVertices,
            Radius = 0.15f
        };

        Transform transform = new( spawnPosition )
        {
            Scale = new Vector3D<float>( 0.2f, 0.2f, 1f )
        };

        Entity entity = World.Spawn( bullet );
        World.Add( entity, transform );
        World.Add( entity, collider );
    }

    private BulletConfig GetBulletConfig()
    {
        foreach ( Entity entity in World.Filter<BulletConfig>() )
        {
            return World.Get<BulletConfig>( entity );
        }

        return BulletConfig.Default;
    }

    private WorldBounds GetWorldBounds()
    {
        foreach ( Entity entity in World.Filter<WorldBounds>() )
        {
            return World.Get<WorldBounds>( entity );
        }

        return WorldBounds.Default;
    }

    private void WrapPosition( ref Vector3D<float> position, WorldBounds bounds )
    {
        float halfWidth = bounds.Width / 2;
        float halfHeight = bounds.Height / 2;

        if ( position.X > halfWidth ) position.X = -halfWidth;
        if ( position.X < -halfWidth ) position.X = halfWidth;
        if ( position.Y > halfHeight ) position.Y = -halfHeight;
        if ( position.Y < -halfHeight ) position.Y = halfHeight;
    }

    private static Mesh CreateBulletMesh( GL gl )
    {
        const int segments = 8;
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        vertices.Add( new Vertex(
            Vector3D<float>.Zero,
            Vector2D<float>.Zero,
            new Vector4D<float>( 1, 1, 1, 1 ),
            0,
            Vector3D<float>.UnitZ
        ) );

        for ( int i = 0; i <= segments; i++ )
        {
            float angle = ( float )i / segments * MathF.PI * 2;
            float x = MathF.Cos( angle ) * 0.15f;
            float y = MathF.Sin( angle ) * 0.15f;

            vertices.Add( new Vertex(
                new Vector3D<float>( x, y, 0f ),
                Vector2D<float>.Zero,
                new Vector4D<float>( 1, 1, 1, 1 ),
                0,
                Vector3D<float>.UnitZ
            ) );
        }

        for ( uint i = 1; i <= segments; i++ )
        {
            indices.Add( 0 );
            indices.Add( i );
            indices.Add( i + 1 );
        }

        Mesh mesh = MeshSystem.CreateMeshFromVertices( gl, vertices.ToArray(), indices.ToArray() );
        mesh.Topology = PrimitiveType.Triangles;

        Material material = new()
        {
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f )
        };
        mesh.Material = material;

        return mesh;
    }
}