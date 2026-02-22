using Aether.Core;
using Maze.Components;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Silk.NET.Maths;

namespace Maze.Systems;

public class MazeFloorSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        MaterialSystem materialSystem = World.GetGlobal<MaterialSystem>();
        MazeMaterials materials = World.GetGlobal<MazeMaterials>();

        foreach ( Entity entity in World.Filter<MazeFloor>().With<Transform>() )
        {
            if ( World.Has<Mesh>( entity ) )
                continue;

            ref MazeFloor floor = ref World.Get<MazeFloor>( entity );

            if ( floor.IsGenerated )
                continue;

            GenerateFloorMesh( entity, ref floor, meshSystem, materials );
            floor.IsGenerated = true;
        }
    }

    private void GenerateFloorMesh( Entity entity, ref MazeFloor floor, MeshSystem meshSystem, MazeMaterials materials )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1, 1, 1, 1 );
        float hx = floor.Size.X / 2f;
        float hz = floor.Size.Y / 2f;
        Vector3D<float> normal = new( 0, 1, 0 );

        float uvScale = 5.0f;
        vertices.Add( new Vertex( new Vector3D<float>( -hx, 0, -hz ), new Vector2D<float>( 0, 0 ), white, 0, normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( hx, 0, -hz ), new Vector2D<float>( uvScale, 0 ), white, 0,
            normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( hx, 0, hz ), new Vector2D<float>( uvScale, uvScale ), white, 0,
            normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( -hx, 0, hz ), new Vector2D<float>( 0, uvScale ), white, 0,
            normal ) );

        indices.AddRange( [ 0, 1, 2, 0, 2, 3 ] );

        World.Add( entity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), materials.GrassMaterial ) );

        floor.IsGenerated = true;
    }
}