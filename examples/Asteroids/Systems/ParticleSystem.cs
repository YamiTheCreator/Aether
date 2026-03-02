using Aether.Core;
using Asteroids.Components;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Asteroids.Systems;

public class ParticleSystem : SystemBase
{
    private readonly Random _random = new();
    private GL? _gl;

    protected override void OnCreate()
    {
        _gl = World.GetGlobal<GL>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        List<Entity> toRemove = [ ];

        foreach ( Entity entity in World.Filter<Particle>() )
        {
            ref Particle particle = ref World.Get<Particle>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            particle.Lifetime -= deltaTime;

            if ( particle.Lifetime <= 0 )
            {
                toRemove.Add( entity );
                continue;
            }

            transform.Position.X += particle.Velocity.X * deltaTime;
            transform.Position.Y += particle.Velocity.Y * deltaTime;

            float alpha = particle.Lifetime / particle.MaxLifetime;
            particle.Color.W = alpha;

            float scale = 0.08f + alpha * 0.15f;
            transform.Scale = new Vector3D<float>( scale, scale, 1f );
        }

        foreach ( Entity entity in toRemove )
        {
            World.Despawn( entity );
        }
    }

    protected override void OnRender()
    {
        if ( _gl == null ) return;

        foreach ( Entity entity in World.Filter<Particle>() )
        {
            ref Particle particle = ref World.Get<Particle>( entity );

            if ( !World.Has<Mesh>( entity ) )
            {
                Mesh particleMesh = CreateCircleMesh( _gl, particle.Color );
                World.Add( entity, particleMesh );
            }

            ref Mesh meshRef = ref World.Get<Mesh>( entity );

            if ( meshRef.Material.HasValue )
            {
                Material mat = meshRef.Material.Value;
                mat.DiffuseColor = new Vector3D<float>( particle.Color.X, particle.Color.Y, particle.Color.Z );
                meshRef.Material = mat;
            }
        }
    }

    protected override void OnDestroy() { }

    public void SpawnEngineParticles( Vector3D<float> position, float angle, int count = 3 )
    {
        for ( int i = 0; i < count; i++ )
        {
            float spreadAngle = angle + ( float )( _random.NextDouble() * 0.6 - 0.3 );
            float speed = 3f + ( float )_random.NextDouble() * 2f;

            Vector2D<float> velocity = new(
                -MathF.Cos( spreadAngle ) * speed,
                -MathF.Sin( spreadAngle ) * speed
            );

            Particle particle = new()
            {
                Velocity = velocity,
                Lifetime = 0.2f + ( float )_random.NextDouble() * 0.15f,
                MaxLifetime = 0.35f,
                Color = new Vector4D<float>( 1f, 0.7f, 0.3f, 1f )
            };

            Transform transform = new( position )
            {
                Scale = new Vector3D<float>( 0.15f, 0.15f, 1f )
            };

            Entity entity = World.Spawn( particle );
            World.Add( entity, transform );
        }
    }

    public void SpawnExplosion( Vector3D<float> position, Vector2D<float> baseVelocity, Vector4D<float> color,
        int count = 8 )
    {
        for ( int i = 0; i < count; i++ )
        {
            float angle = ( float )_random.NextDouble() * MathF.PI * 2;
            float speed = 2f + ( float )_random.NextDouble() * 3f;

            Vector2D<float> velocity = new(
                baseVelocity.X + MathF.Cos( angle ) * speed,
                baseVelocity.Y + MathF.Sin( angle ) * speed
            );

            Particle particle = new()
            {
                Velocity = velocity,
                Lifetime = 1f + ( float )_random.NextDouble() * 1f,
                MaxLifetime = 2f,
                Color = color
            };

            Transform transform = new( position )
            {
                Scale = new Vector3D<float>( 0.2f, 0.2f, 1f ),
                Rotation = Quaternion<float>.CreateFromAxisAngle( Vector3D<float>.UnitZ,
                    ( float )_random.NextDouble() * MathF.PI * 2 )
            };

            Entity entity = World.Spawn( particle );
            World.Add( entity, transform );
        }
    }

    private static Mesh CreateCircleMesh( GL gl, Vector4D<float> color )
    {
        const int segments = 16;
        const float radius = 0.5f;

        Vertex[] vertices = CreateCircleVertices( segments, radius );
        uint[] indices = CreateTriangleFanIndices( segments );

        Mesh mesh = MeshSystem.CreateMeshFromVertices( gl, vertices, indices );
        mesh.Topology = PrimitiveType.Triangles;
        mesh.Material = CreateColoredMaterial( color );

        return mesh;
    }

    private static Vertex[] CreateCircleVertices( int segments, float radius )
    {
        List<Vertex> vertices = [ ];

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
            float x = MathF.Cos( angle ) * radius;
            float y = MathF.Sin( angle ) * radius;

            vertices.Add( new Vertex(
                new Vector3D<float>( x, y, 0f ),
                Vector2D<float>.Zero,
                new Vector4D<float>( 1, 1, 1, 1 ),
                0,
                Vector3D<float>.UnitZ
            ) );
        }

        return vertices.ToArray();
    }

    private static uint[] CreateTriangleFanIndices( int segments )
    {
        List<uint> indices = [ ];

        for ( uint i = 1; i <= segments; i++ )
        {
            indices.Add( 0 );
            indices.Add( i );
            indices.Add( i + 1 );
        }

        return indices.ToArray();
    }

    private static Material CreateColoredMaterial( Vector4D<float> color )
    {
        return new Material
        {
            DiffuseColor = new Vector3D<float>( color.X, color.Y, color.Z )
        };
    }
}