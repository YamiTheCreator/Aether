using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MathShapes2D.Components;

namespace MathShapes2D.Systems;

public class CardioidPolarSystem : SystemBase
{
    private MeshSystem? _meshSystem;
    private MaterialSystem? _materialSystem;

    protected override void OnCreate()
    {
        _meshSystem = World.GetGlobal<MeshSystem>();
        _materialSystem = World.GetGlobal<MaterialSystem>();
    }

    protected override void OnUpdate( float deltaTime ) { }

    protected override void OnRender()
    {
        if ( _meshSystem == null || _materialSystem == null ) return;

        foreach ( Entity entity in World.Filter<CardioidPolar>() )
        {
            if ( !World.Has<Transform>( entity ) )
                continue;

            ref CardioidPolar cardioid = ref World.Get<CardioidPolar>( entity );

            if ( cardioid.IsGenerated )
                continue;

            GenerateCardioid( entity, ref cardioid, _meshSystem, _materialSystem );
            cardioid.IsGenerated = true;
        }
    }

    protected override void OnDestroy() { }

    private void GenerateCardioid( Entity entity, ref CardioidPolar cardioid, MeshSystem meshSystem,
        MaterialSystem materialSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> color = new( 1f, 1f, 1f, 1f );

        // r = a(1 - cos(φ)) на интервале φ∈[0;2π]
        for ( int i = 0; i <= cardioid.Segments; i++ )
        {
            float phi = 2f * MathF.PI * i / cardioid.Segments;
            float r = cardioid.Scale * ( 1f - MathF.Cos( phi ) );

            float x = r * MathF.Cos( phi );
            float y = r * MathF.Sin( phi );

            vertices.Add( new Vertex(
                new Vector3D<float>( x, y, 0f ),
                new Vector2D<float>( 0, 0 ),
                color,
                0,
                new Vector3D<float>( 0, 0, 1 )
            ) );
        }

        for ( uint i = 0; i < cardioid.Segments; i++ )
        {
            indices.Add( i );
            indices.Add( i + 1 );
        }

        Material material = materialSystem.CreateUnlit( new Vector3D<float>( 1f, 1f, 1f ) );
        Mesh mesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material );
        mesh.Topology = Silk.NET.OpenGL.PrimitiveType.Lines;
        World.Add( entity, mesh );
    }
}