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
        return CheckAabbCollision(
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

                if ( !wallsByMaterial.TryGetValue( material, out (List<Vertex> vertices, List<uint> indices) value ) )
                {
                    value =  ( [ ], [ ] );
                    wallsByMaterial[ material ] = value;
                }

                (List<Vertex> vertices, List<uint> indices) = value;
                AddWallQuads( vertices, indices, x, z );
            }
        }

        foreach ( (Material material, (List<Vertex> vertices, List<uint> indices)) in wallsByMaterial )
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

    // Добавляем стены, не учитывая пересекающиеся грани
    private void AddWallQuads( List<Vertex> vertices, List<uint> indices, int x, int z )
    {
        Vector4D<float> white = new( 1, 1, 1, 1 );
        float x0 = x;
        float x1 = x + 1;
        float z0 = z;
        float z1 = z + 1;
        const float y0 = 0;
        const float y1 = 1;

        // -Z
        if ( !IsWall( x, z - 1 ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x0, y0, z0 ), new Vector3D<float>( x1, y0, z0 ),
                new Vector3D<float>( x1, y1, z0 ), new Vector3D<float>( x0, y1, z0 ),
                new Vector3D<float>( 0, 0, -1 ), white );
        }

        // +Z
        if ( !IsWall( x, z + 1 ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x1, y0, z1 ), new Vector3D<float>( x0, y0, z1 ),
                new Vector3D<float>( x0, y1, z1 ), new Vector3D<float>( x1, y1, z1 ),
                new Vector3D<float>( 0, 0, 1 ), white );
        }

        // -X
        if ( !IsWall( x - 1, z ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x0, y0, z1 ), new Vector3D<float>( x0, y0, z0 ),
                new Vector3D<float>( x0, y1, z0 ), new Vector3D<float>( x0, y1, z1 ),
                new Vector3D<float>( -1, 0, 0 ), white );
        }

        // +X
        if ( !IsWall( x + 1, z ) )
        {
            AddQuad( vertices, indices,
                new Vector3D<float>( x1, y0, z0 ), new Vector3D<float>( x1, y0, z1 ),
                new Vector3D<float>( x1, y1, z1 ), new Vector3D<float>( x1, y1, z0 ),
                new Vector3D<float>( 1, 0, 0 ), white );
        }
    }

    private Material SelectWallMaterial( Materials materials, int x, int z )
    {
        // Делим лабиринт 15x15 на 6 зон (3x2 сетка по 5 тайлов)
        return ( x, z ) switch
        {
            (< 5, < 5) => materials.Metal,           // Верхний левый
            (  < 10, < 5) => materials.Stone, // Верхний центр
            (>= 10, < 5) => materials.Wood,          // Верхний правый
            (< 5,  < 10) => materials.Steel, // Средний левый
            ( < 10,  < 10) => materials.Oak, // Средний центр
            (>= 10,  < 10) => materials.Rock, // Средний правый
            (< 5, >= 10) => materials.Metal,  // Нижний левый
            ( < 10, >= 10) => materials.Stone, // Нижний центр
            _ => materials.Wood                       // Нижний правый
        };
    }

    private void GenerateFloor( MeshSystem meshSystem, Materials materials )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1, 1, 1, 1 );
        Vector3D<float> normal = new( 0, 1, 0 );
        const float uvScale = 5.0f;

        vertices.Add( new Vertex( new Vector3D<float>( 0, 0, 0 ), new Vector2D<float>( 0, 0 ), white, 0, normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( _grid.Width, 0, 0 ), new Vector2D<float>( uvScale, 0 ), white, 0,
            normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( _grid.Width, 0, _grid.Height ),
            new Vector2D<float>( uvScale, uvScale ), white, 0, normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( 0, 0, _grid.Height ), new Vector2D<float>( 0, uvScale ), white,
            0, normal ) );

        indices.AddRange( [ 0, 1, 2, 0, 2, 3 ] );

        Entity floorEntity = World.Spawn();
        World.Add( floorEntity, new Transform
        {
            Position = Vector3D<float>.Zero,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( floorEntity,
            meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), materials.Grass ) );
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

        indices.AddRange( [ offset, offset + 1, offset + 2, offset, offset + 2, offset + 3 ] );
    }
    
    // AABB проверка коллизий для простых случаев в 2D через круг и тайлы
    // Проверяем пересечение круга с сеткой тайлов, используя ближайшую точку на тайле
    public static bool CheckAabbCollision<T>( T grid, float worldX, float worldZ, float radius,
        Func<T, int, int, bool> isWallFunc, int gridWidth, int gridHeight )
    {
        int minX = ( int )Math.Floor( worldX - radius );
        int maxX = ( int )Math.Ceiling( worldX + radius );
        int minZ = ( int )Math.Floor( worldZ - radius );
        int maxZ = ( int )Math.Ceiling( worldZ + radius );

        for ( int x = minX; x <= maxX; x++ )
        {
            for ( int z = minZ; z <= maxZ; z++ )
            {
                // Выходим за границы сетки - коллизия
                if ( x < 0 || x >= gridWidth || z < 0 || z >= gridHeight )
                    return true;

                // Если стена
                if ( isWallFunc( grid, x, z ) )
                {
                    // Находим ближвайшую точку на тайле к центру круга
                    float closestX = Math.Clamp( worldX, x, x + 1 );
                    float closestZ = Math.Clamp( worldZ, z, z + 1 );

                    // Вычисялем расстояние от центра до этой точки
                    float dx = worldX - closestX;
                    float dz = worldZ - closestZ;
                    float distSq = dx * dx + dz * dz;
                    
                    // Если расстояние меньше радиуса - есть коллизия
                    if ( distSq < radius * radius )
                        return true;
                }
            }
        }

        return false;
    }
}