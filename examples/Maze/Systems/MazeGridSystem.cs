using Aether.Core;
using Maze.Components;
using Graphics.Components;
using Silk.NET.Maths;

namespace Maze.Systems;

public class MazeGridSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        MazeMaterials materials = World.GetGlobal<MazeMaterials>();

        foreach ( Entity entity in World.Filter<MazeGrid>() )
        {
            ref MazeGrid grid = ref World.Get<MazeGrid>( entity );

            if ( grid.IsGenerated )
                continue;

            GenerateWallEntities( ref grid, materials );
            grid.IsGenerated = true;
        }
    }

    private void GenerateWallEntities( ref MazeGrid grid, MazeMaterials materials )
    {
        for ( int x = 0; x < grid.Width; x++ )
        {
            for ( int z = 0; z < grid.Height; z++ )
            {
                if ( grid.Layout[ z, x ] != 1 )
                    continue;

                Material material;

                if ( x < 7 && z < 7 )
                {
                    material = materials.BrickMaterial;
                }
                else if ( x >= 7 && z < 7 )
                {
                    material = materials.StoneMaterial;
                }
                else if ( x < 7 && z >= 7 )
                {
                    material = materials.TileMaterial;
                }
                else
                {
                    material = materials.SandstoneMaterial;
                }

                Entity wallEntity = World.Spawn();
                World.Add( wallEntity, new Transform
                {
                    Position = new Vector3D<float>( x + 0.5f, 0.5f, z + 0.5f ),
                    Rotation = Quaternion<float>.Identity,
                    Scale = Vector3D<float>.One
                } );
                World.Add( wallEntity, new MazeWall
                {
                    Material = material
                } );
            }
        }
    }
}