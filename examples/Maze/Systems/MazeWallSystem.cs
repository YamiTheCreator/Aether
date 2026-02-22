using Aether.Core;
using Maze.Components;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Silk.NET.Maths;

namespace Maze.Systems;

public class MazeWallSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        MaterialSystem materialSystem = World.GetGlobal<MaterialSystem>();

        foreach ( Entity entity in World.Filter<MazeWall>().With<Transform>() )
        {
            if ( World.Has<Mesh>( entity ) )
                continue;

            ref MazeWall wall = ref World.Get<MazeWall>( entity );

            GenerateWallMesh( entity, ref wall, meshSystem );
        }
    }

    private void GenerateWallMesh( Entity entity, ref MazeWall wall, MeshSystem meshSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        const float s = 0.5f;
        Vector4D<float> white = new( 1, 1, 1, 1 );

        AddQuadWithUv( vertices, indices,
            new Vector3D<float>( -s, -s, -s ), new Vector3D<float>( s, -s, -s ),
            new Vector3D<float>( s, s, -s ), new Vector3D<float>( -s, s, -s ),
            new Vector3D<float>( 0, 0, -1 ), white );

        AddQuadWithUv( vertices, indices,
            new Vector3D<float>( s, -s, s ), new Vector3D<float>( -s, -s, s ),
            new Vector3D<float>( -s, s, s ), new Vector3D<float>( s, s, s ),
            new Vector3D<float>( 0, 0, 1 ), white );

        AddQuadWithUv( vertices, indices,
            new Vector3D<float>( -s, -s, s ), new Vector3D<float>( -s, -s, -s ),
            new Vector3D<float>( -s, s, -s ), new Vector3D<float>( -s, s, s ),
            new Vector3D<float>( -1, 0, 0 ), white );

        AddQuadWithUv( vertices, indices,
            new Vector3D<float>( s, -s, -s ), new Vector3D<float>( s, -s, s ),
            new Vector3D<float>( s, s, s ), new Vector3D<float>( s, s, -s ),
            new Vector3D<float>( 1, 0, 0 ), white );

        AddQuadWithUv( vertices, indices,
            new Vector3D<float>( -s, s, -s ), new Vector3D<float>( s, s, -s ),
            new Vector3D<float>( s, s, s ), new Vector3D<float>( -s, s, s ),
            new Vector3D<float>( 0, 1, 0 ), white );

        AddQuadWithUv( vertices, indices,
            new Vector3D<float>( -s, -s, s ), new Vector3D<float>( s, -s, s ),
            new Vector3D<float>( s, -s, -s ), new Vector3D<float>( -s, -s, -s ),
            new Vector3D<float>( 0, -1, 0 ), white );

        World.Add( entity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), wall.Material ) );
    }

    private void AddQuadWithUv( List<Vertex> vertices, List<uint> indices,
        Vector3D<float> v0, Vector3D<float> v1, Vector3D<float> v2, Vector3D<float> v3,
        Vector3D<float> normal, Vector4D<float> color )
    {
        uint offset = ( uint )vertices.Count;

        vertices.Add( new Vertex( v0, new Vector2D<float>( 0, 0 ), color, 0, normal ) );
        vertices.Add( new Vertex( v1, new Vector2D<float>( 1, 0 ), color, 0, normal ) );
        vertices.Add( new Vertex( v2, new Vector2D<float>( 1, 1 ), color, 0, normal ) );
        vertices.Add( new Vertex( v3, new Vector2D<float>( 0, 1 ), color, 0, normal ) );

        indices.AddRange( [ offset, offset + 1, offset + 2, offset, offset + 2, offset + 3 ] );
    }
}