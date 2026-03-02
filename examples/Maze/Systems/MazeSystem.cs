using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Maze.Components;
using Silk.NET.Maths;

namespace Maze.Systems;

public class MazeSystem : SystemBase
{
    private Grid _grid;

    protected override void OnCreate()
    {
        _grid = GetGrid();

        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        Materials materials = World.GetGlobal<Materials>();

        GenerateWalls( meshSystem, materials );
        GenerateFloor( meshSystem, materials );
    }

    protected override void OnUpdate( float deltaTime ) { }

    protected override void OnRender() { }

    protected override void OnDestroy() { }

    private Grid GetGrid()
    {
        foreach ( Entity entity in World.Filter<Grid>() )
        {
            return World.Get<Grid>( entity );
        }

        throw new InvalidOperationException( "Grid component not found" );
    }

    public bool IsWall( int x, int z )
    {
        if ( x < 0 || x >= _grid.Width || z < 0 || z >= _grid.Height )
            return true;
        return _grid.Layout[ z, x ] == 1;
    }

    public bool CheckCollision( float worldX, float worldZ, float radius )
    {
        return CollisionSystem.CheckCircleGridCollision(
            this,
            worldX,
            worldZ,
            radius,
            ( system, x, z ) => system.IsWall( x, z ),
            _grid.Width,
            _grid.Height
        );
    }

    private void GenerateWalls( MeshSystem meshSystem, Materials materials )
    {
        Dictionary<Material, (List<Vertex> vertices, List<uint> indices)> wallsByMaterial = new();

        for ( int x = 0; x < _grid.Width; x++ )
        {
            for ( int z = 0; z < _grid.Height; z++ )
            {
                if ( !IsWall( x, z ) )
                    continue;

                Material material = SelectWallMaterial( materials, x, z );
                
                if ( !wallsByMaterial.ContainsKey( material ) )
                {
                    wallsByMaterial[ material ] = ( new List<Vertex>(), new List<uint>() );
                }

                var (vertices, indices) = wallsByMaterial[ material ];
                AddWallQuads( vertices, indices, x, z );
            }
        }

        foreach ( var (material, (vertices, indices)) in wallsByMaterial )
        {
            if ( vertices.Count > 0 )
            {
                Entity wallsEntity = World.Spawn();
                World.Add( wallsEntity, new Transform
                {
                    Position = Vector3D<float>.Zero,
                    Rotation = Quaternion<float>.Identity,
                    Scale = Vector3D<float>.One
                } );
                World.Add( wallsEntity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material ) );
            }
        }
    }

    private void AddWallQuads( List<Vertex> vertices, List<uint> indices, int x, int z )
    {
        Vector4D<float> white = new( 1, 1, 1, 1 );
        float x0 = x;
        float x1 = x + 1;
        float z0 = z;
        float z1 = z + 1;
        float y0 = 0;
        float y1 = 1;

        // Front (-Z)
        if ( !IsWall( x, z - 1 ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x0, y0, z0 ), new Vector3D<float>( x1, y0, z0 ),
                new Vector3D<float>( x1, y1, z0 ), new Vector3D<float>( x0, y1, z0 ),
                new Vector3D<float>( 0, 0, -1 ), white );
        }

        // Back (+Z)
        if ( !IsWall( x, z + 1 ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x1, y0, z1 ), new Vector3D<float>( x0, y0, z1 ),
                new Vector3D<float>( x0, y1, z1 ), new Vector3D<float>( x1, y1, z1 ),
                new Vector3D<float>( 0, 0, 1 ), white );
        }

        // Left (-X)
        if ( !IsWall( x - 1, z ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x0, y0, z1 ), new Vector3D<float>( x0, y0, z0 ),
                new Vector3D<float>( x0, y1, z0 ), new Vector3D<float>( x0, y1, z1 ),
                new Vector3D<float>( -1, 0, 0 ), white );
        }

        // Right (+X)
        if ( !IsWall( x + 1, z ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x1, y0, z0 ), new Vector3D<float>( x1, y0, z1 ),
                new Vector3D<float>( x1, y1, z1 ), new Vector3D<float>( x1, y1, z0 ),
                new Vector3D<float>( 1, 0, 0 ), white );
        }

        // Top (+Y)
        AddQuad( vertices, indices,
            new Vector3D<float>( x0, y1, z0 ), new Vector3D<float>( x1, y1, z0 ),
            new Vector3D<float>( x1, y1, z1 ), new Vector3D<float>( x0, y1, z1 ),
            new Vector3D<float>( 0, 1, 0 ), white );

        // Bottom (-Y)
        AddQuad( vertices, indices,
            new Vector3D<float>( x0, y0, z1 ), new Vector3D<float>( x1, y0, z1 ),
            new Vector3D<float>( x1, y0, z0 ), new Vector3D<float>( x0, y0, z0 ),
            new Vector3D<float>( 0, -1, 0 ), white );
    }

    private Material SelectWallMaterial( Materials materials, int x, int z )
    {
        return ( x, z ) switch
        {
            ( < 7, < 7 ) => materials.BrickMaterial,
            ( >= 7, < 7 ) => materials.StoneMaterial,
            ( < 7, >= 7 ) => materials.TileMaterial,
            _ => materials.SandstoneMaterial
        };
    }

    private void GenerateFloor( MeshSystem meshSystem, Materials materials )
    {
        List<Vertex> vertices = [];
        List<uint> indices = [];

        Vector4D<float> white = new( 1, 1, 1, 1 );
        Vector3D<float> normal = new( 0, 1, 0 );
        float uvScale = 5.0f;

        vertices.Add( new Vertex( new Vector3D<float>( 0, 0, 0 ), new Vector2D<float>( 0, 0 ), white, 0, normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( _grid.Width, 0, 0 ), new Vector2D<float>( uvScale, 0 ), white, 0, normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( _grid.Width, 0, _grid.Height ), new Vector2D<float>( uvScale, uvScale ), white, 0, normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( 0, 0, _grid.Height ), new Vector2D<float>( 0, uvScale ), white, 0, normal ) );

        indices.AddRange( [0, 1, 2, 0, 2, 3] );

        Entity floorEntity = World.Spawn();
        World.Add( floorEntity, new Transform
        {
            Position = Vector3D<float>.Zero,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( floorEntity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), materials.GrassMaterial ) );
    }

    private void AddQuad( List<Vertex> vertices, List<uint> indices,
        Vector3D<float> v0, Vector3D<float> v1, Vector3D<float> v2, Vector3D<float> v3,
        Vector3D<float> normal, Vector4D<float> color )
    {
        uint offset = ( uint )vertices.Count;

        float width = Vector3D.Distance( v0, v1 );
        float height = Vector3D.Distance( v1, v2 );

        vertices.Add( new Vertex( v0, new Vector2D<float>( 0, 0 ), color, 0, normal ) );
        vertices.Add( new Vertex( v1, new Vector2D<float>( width, 0 ), color, 0, normal ) );
        vertices.Add( new Vertex( v2, new Vector2D<float>( width, height ), color, 0, normal ) );
        vertices.Add( new Vertex( v3, new Vector2D<float>( 0, height ), color, 0, normal ) );

        indices.AddRange( [offset, offset + 1, offset + 2, offset, offset + 2, offset + 3] );
    }
}
