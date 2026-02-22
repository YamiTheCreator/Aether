using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MathShapes2D.Components;

namespace MathShapes2D.Systems;

public class GeometryCircleSystem : SystemBase
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

        foreach ( Entity entity in World.Filter<GeometryCircle>() )
        {
            if ( !World.Has<Transform>( entity ) )
                continue;

            ref GeometryCircle circle = ref World.Get<GeometryCircle>( entity );

            if ( circle.IsGenerated )
                continue;

            GeneratePoint( entity, _meshSystem );
            circle.IsGenerated = true;
        }
    }

    protected override void OnDestroy() { }

    private void GeneratePoint( Entity entity, MeshSystem meshSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> color = new( 1f, 1f, 1f, 1f );
        
        vertices.Add( new Vertex(
            new Vector3D<float>( 0f, 0f, 0f ),
            new Vector2D<float>( 0, 0 ),
            color,
            0,
            new Vector3D<float>( 0, 0, 1 )
        ) );

        indices.Add( 0 );

        Material material = World.Get<Material>( entity );

        Mesh mesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material );
        mesh.Topology = Silk.NET.OpenGL.PrimitiveType.Points;
        World.Add( entity, mesh );
    }
}