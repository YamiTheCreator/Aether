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

            if ( bottle.IsGenerated )
                continue;

            GenerateBottleMesh( entity, ref bottle, _meshSystem, _materialSystem );
            bottle.IsGenerated = true;
        }
    }

    protected override void OnDestroy() { }

    private void GenerateBottleMesh( Entity entity, ref KleinBottle bottle, MeshSystem meshSystem,
        MaterialSystem materialSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        // Вычисляем min/max Y для маппинга цвета
        for ( int i = 0; i <= bottle.USegments; i++ )
        {
            float u = ( float )i / bottle.USegments * MathF.PI * 2f;
            for ( int j = 0; j <= bottle.VSegments; j++ )
            {
                float v = ( float )j / bottle.VSegments * MathF.PI * 2f;
                float r = 4f * ( 1f - MathF.Cos( v ) / 2f );
                float y = 16f * MathF.Sin( v ) * bottle.Scale * 0.1f;
                minY = MathF.Min( minY, y );
                maxY = MathF.Max( maxY, y );
            }
        }

        // Вычисляем координаты
        for ( int i = 0; i <= bottle.USegments; i++ )
        {
            float u = ( float )i / bottle.USegments * MathF.PI * 2f;
            for ( int j = 0; j <= bottle.VSegments; j++ )
            {
                float v = ( float )j / bottle.VSegments * MathF.PI * 2f;
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

                x *= bottle.Scale * 0.1f;
                y *= bottle.Scale * 0.1f;
                z *= bottle.Scale * 0.1f;

                Vector3D<float> pos = new( x, -y, z );

                // Вычисление нормалей численным методом
                const float dv = 0.01f;

                // Касательный вектор по v
                float r1 = 4f * ( 1f - MathF.Cos( v + dv ) / 2f );
                float x1, y1, z1;

                if ( v + dv < MathF.PI )
                {
                    x1 = 6f * MathF.Cos( v + dv ) * ( 1f + MathF.Sin( v + dv ) ) +
                         r1 * MathF.Cos( v + dv ) * MathF.Cos( u );
                    y1 = 16f * MathF.Sin( v + dv );
                    z1 = r1 * MathF.Sin( u );
                }
                else
                {
                    x1 = 6f * MathF.Cos( v + dv ) * ( 1f + MathF.Sin( v + dv ) ) + r1 * MathF.Cos( u + MathF.PI );
                    y1 = 16f * MathF.Sin( v + dv );
                    z1 = r1 * MathF.Sin( u );
                }

                x1 *= bottle.Scale * 0.1f;
                y1 *= bottle.Scale * 0.1f;
                z1 *= bottle.Scale * 0.1f;

                // Касательный вектор по u
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

                x2 *= bottle.Scale * 0.1f;
                y2 *= bottle.Scale * 0.1f;
                z2 *= bottle.Scale * 0.1f;

                Vector3D<float> tangentV = new( x1 - x, -( y1 - y ), z1 - z );
                Vector3D<float> tangentU = new( x2, -y2, z2 );
                Vector3D<float> normal = Vector3D.Normalize( Vector3D.Cross( tangentU, tangentV ) );

                // Цветовой градиент на основе Y координаты (более явный)
                float t = ( y - minY ) / ( maxY - minY );
                Vector4D<float> color = new(
                    0.2f + t * 0.8f, // От темно-красного к яркому
                    0.1f + t * 0.3f, // Немного зеленого
                    1f - t * 0.7f, // От синего к фиолетовому
                    1.0f // Полностью непрозрачный
                );

                vertices.Add( new Vertex( pos,
                    new Vector2D<float>( ( float )i / bottle.USegments, ( float )j / bottle.VSegments ), color, 0,
                    normal ) );
            }
        }

        // Создаем индексы
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

        // Добавляем меш и материал к сущности
        World.Add( entity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() ) );

        Material material = materialSystem.CreateUnlit( new Vector3D<float>( 1, 1, 1 ) );
        material.Alpha = 1.0f; // Полностью непрозрачный
        World.Add( entity, material );
    }
}