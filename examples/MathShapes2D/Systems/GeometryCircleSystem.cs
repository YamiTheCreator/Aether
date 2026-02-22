using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MathShapes2D.Components;

namespace MathShapes2D.Systems;

/// <summary>
/// Geometry circle system - generates single point that geometry shader converts to circle
/// </summary>
public class GeometryCircleSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();

        foreach ( Entity entity in World.Filter<GeometryCircle>() )
        {
            if ( !World.Has<Transform>( entity ) )
                continue;

            ref GeometryCircle circle = ref World.Get<GeometryCircle>( entity );

            if ( circle.IsGenerated )
                continue;

            GeneratePoint( entity, ref circle, meshSystem );
            circle.IsGenerated = true;
        }
    }

    private void GeneratePoint( Entity entity, ref GeometryCircle circle, MeshSystem meshSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> color = new( 1f, 1f, 1f, 1f );

        // Создаем одну точку в центре - геометрический шейдер превратит её в окружность
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