using Aether.Core;
using Camera3D.Components;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Silk.NET.Maths;

namespace Camera3D.Systems;

public class KleinBottleSystem : SystemBase
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

        foreach ( Entity entity in World.Filter<KleinBottle, Transform>() )
        {
            if ( World.Has<Mesh>( entity ) )
                continue;

            ref KleinBottle bottle = ref World.Get<KleinBottle>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            if ( bottle.IsGenerated )
                continue;

            GenerateBottleMesh( entity, ref bottle, ref transform, _meshSystem, _materialSystem );
            bottle.IsGenerated = true;
        }
    }

    protected override void OnDestroy() { }

    // Генерируем полный меш бутылки Клейна: вычисляем границы, создаём вершины и индексы
    private void GenerateBottleMesh( Entity entity, ref KleinBottle bottle, ref Transform transform,
        MeshSystem meshSystem, MaterialSystem materialSystem )
    {
        float scale = transform.Scale.X;
        (float minY, float maxY) = CalculateYBounds( bottle, scale );
        List<Vertex> vertices = GenerateVertices( bottle, scale, minY, maxY );
        List<uint> indices = GenerateIndices( bottle );

        AddMeshToEntity( entity, vertices, indices, meshSystem, materialSystem );
    }

    // Вычисляем минимальную и максимальную Y координату для градиента цвета
    private (float minY, float maxY) CalculateYBounds( KleinBottle bottle, float scale )
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for ( int i = 0; i <= bottle.USegments; i++ )
        {
            for ( int j = 0; j <= bottle.VSegments; j++ )
            {
                float v = ( float )j / bottle.VSegments * MathF.PI * 2f;
                float y = 16f * MathF.Sin( v ) * scale * 0.1f;
                minY = MathF.Min( minY, y );
                maxY = MathF.Max( maxY, y );
            }
        }

        return (minY, maxY);
    }

    // Генерируем все вершины бутылки Клейна с позициями, нормалями, цветами и UV координатами
    private List<Vertex> GenerateVertices( KleinBottle bottle, float scale, float minY, float maxY )
    {
        List<Vertex> vertices = [ ];

        for ( int i = 0; i <= bottle.USegments; i++ )
        {
            float u = ( float )i / bottle.USegments * MathF.PI * 2f;
            for ( int j = 0; j <= bottle.VSegments; j++ )
            {
                float v = ( float )j / bottle.VSegments * MathF.PI * 2f;
                
                Vector3D<float> position = CalculateKleinBottlePosition( u, v, scale );
                Vector3D<float> normal = CalculateKleinBottleNormal( u, v, scale );
                Vector4D<float> color = CalculateVertexColor( position.Y, minY, maxY );
                Vector2D<float> uv = new( ( float )i / bottle.USegments, ( float )j / bottle.VSegments );

                vertices.Add( new Vertex( position, uv, color, 0, normal ) );
            }
        }

        return vertices;
    }

    // Вычисляем позицию точки на поверхности бутылки Клейна по параметрическим координатам u и v
    private Vector3D<float> CalculateKleinBottlePosition( float u, float v, float scale )
    {
        float r = 4f * ( 1f - MathF.Cos( v ) / 2f );
        float x, y, z;

        if ( v < MathF.PI )
        {
            x = 6f * MathF.Cos( v ) * ( 1f + MathF.Sin( v ) ) + r * MathF.Cos( v ) * MathF.Cos( u );
            y = 16f * MathF.Sin( v );
            z = r * MathF.Sin( u );
        }
        else
        {
            x = 6f * MathF.Cos( v ) * ( 1f + MathF.Sin( v ) ) + r * MathF.Cos( u + MathF.PI );
            y = 16f * MathF.Sin( v );
            z = r * MathF.Sin( u );
        }

        return new Vector3D<float>( x * scale * 0.1f, -y * scale * 0.1f, z * scale * 0.1f );
    }

    // Вычисляем нормаль к поверхности через векторное произведение касательных векторов
    private Vector3D<float> CalculateKleinBottleNormal( float u, float v, float scale )
    {
        const float dv = 0.01f;
        
        Vector3D<float> pos = CalculateKleinBottlePosition( u, v, scale );
        Vector3D<float> posV = CalculateKleinBottlePosition( u, v + dv, scale );
        
        float r = 4f * ( 1f - MathF.Cos( v ) / 2f );
        Vector3D<float> tangentU = CalculateTangentU( u, v, r, scale );
        // производная по вертикали сложная, так как функция кусочная и много параметров
        // легче использовать приближение
        Vector3D<float> tangentV = posV - pos;

        return Vector3D.Normalize( Vector3D.Cross( tangentU, tangentV ) );
    }

    // Вычисляем касательный вектор по параметру u аналитически
    private Vector3D<float> CalculateTangentU( float u, float v, float r, float scale )
    {
        float x2, y2, z2;

        if ( v < MathF.PI )
        {
            x2 = -r * MathF.Cos( v ) * MathF.Sin( u );
            y2 = 0f;
            z2 = r * MathF.Cos( u );
        }
        else
        {
            x2 = -r * MathF.Sin( u + MathF.PI );
            y2 = 0f;
            z2 = r * MathF.Cos( u );
        }

        return new Vector3D<float>( x2 * scale * 0.1f, -y2 * scale * 0.1f, z2 * scale * 0.1f );
    }

    // Вычисляем цвет вершины на основе её Y координаты для создания градиента
    private Vector4D<float> CalculateVertexColor( float y, float minY, float maxY )
    {
        // Нормализация
        float t = ( y - minY ) / ( maxY - minY );
        return new Vector4D<float>(
            0.2f + t * 0.8f,
            0.1f + t * 0.3f,
            1f - t * 0.7f,
            1.0f
        );
    }

    // Генерируем индексы для треугольников, соединяющих вершины в сетку
    private List<uint> GenerateIndices( KleinBottle bottle )
    {
        List<uint> indices = [ ];

        for ( int i = 0; i < bottle.USegments; i++ )
        {
            for ( int j = 0; j < bottle.VSegments; j++ )
            {
                uint topLeft = ( uint )( i * ( bottle.VSegments + 1 ) + j );
                uint topRight = ( uint )( ( i + 1 ) * ( bottle.VSegments + 1 ) + j );
                uint bottomLeft = ( uint )( i * ( bottle.VSegments + 1 ) + j + 1 );
                uint bottomRight = ( uint )( ( i + 1 ) * ( bottle.VSegments + 1 ) + j + 1 );

                indices.Add( topLeft );
                indices.Add( bottomLeft );
                indices.Add( topRight );

                indices.Add( topRight );
                indices.Add( bottomLeft );
                indices.Add( bottomRight );
            }
        }

        return indices;
    }

    // Добавляем меш и материал к сущности
    private void AddMeshToEntity( Entity entity, List<Vertex> vertices, List<uint> indices, 
        MeshSystem meshSystem, MaterialSystem materialSystem )
    {
        World.Add( entity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() ) );

        Material material = materialSystem.CreateUnlit( new Vector3D<float>( 1, 1, 1 ) );
        material.Alpha = 1.0f;
        World.Add( entity, material );
    }
}