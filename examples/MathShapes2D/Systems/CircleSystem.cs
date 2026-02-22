using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MathShapes2D.Components;

namespace MathShapes2D.Systems;

public class CircleSystem : SystemBase
{
    private MeshSystem? _meshSystem;

    protected override void OnCreate()
    {
        _meshSystem = World.GetGlobal<MeshSystem>();
    }

    protected override void OnUpdate( float deltaTime ) { }

    protected override void OnRender()
    {
        if ( _meshSystem == null ) return;

        foreach ( Entity entity in World.Filter<Circle>() )
        {
            if ( !World.Has<Transform>( entity ) )
                continue;

            ref Circle circle = ref World.Get<Circle>( entity );

            if ( circle.IsGenerated )
                continue;

            GenerateCardioid( entity, ref circle, _meshSystem );
            circle.IsGenerated = true;
        }
    }

    protected override void OnDestroy() { }

    private void GenerateCardioid( Entity entity, ref Circle circle, MeshSystem meshSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> color = new( 1f, 1f, 1f, 1f );

        float halfLength = MathF.PI;

        for ( int i = 0; i <= circle.Segments; i++ )
        {
            float x = -halfLength + ( 2f * halfLength * i / circle.Segments );
            float y = 0f;

            vertices.Add( new Vertex(
                new Vector3D<float>( x, y, 0f ),
                new Vector2D<float>( 0, 0 ),
                color,
                0,
                new Vector3D<float>( 0, 0, 1 )
            ) );
        }

        for ( uint i = 0; i <= circle.Segments; i++ )
        {
            indices.Add( i );
        }

        Material material = World.Get<Material>( entity );

        Mesh mesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material );
        mesh.Topology = Silk.NET.OpenGL.PrimitiveType.LineStrip;
        World.Add( entity, mesh );
    }
}