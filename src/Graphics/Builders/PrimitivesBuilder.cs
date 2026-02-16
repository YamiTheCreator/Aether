using System.Numerics;
using Aether.Core.Structures;

namespace Graphics.Builders;

public static class PrimitivesBuilder
{
    public static QuadVertex[] CreateCircle( Vector3 center, float radius, Vector4 color, int segments = 32 )
    {
        int vertexCount = segments + 2; // center + segments + closing vertex
        QuadVertex[] vertices = new QuadVertex[ vertexCount ];
        
        vertices[ 0 ] = new( center, new Vector2( 0.5f, 0.5f ), color );
        
        for ( int i = 0; i <= segments; i++ )
        {
            float angle = ( float )i / segments * MathF.PI * 2f;
            float x = center.X + MathF.Cos( angle ) * radius;
            float y = center.Y + MathF.Sin( angle ) * radius;
            float u = ( MathF.Cos( angle ) + 1f ) * 0.5f;
            float v = ( MathF.Sin( angle ) + 1f ) * 0.5f;

            vertices[ i + 1 ] = new QuadVertex( new Vector3( x, y, center.Z ), new Vector2( u, v ), color );
        }

        return vertices;
    }

    public static uint[] CreateCircleIndices( int segments )
    {
        int indexCount = segments * 3;
        uint[] indices = new uint[ indexCount ];
        
        for ( int i = 0; i < segments; i++ )
        {
            int offset = i * 3;
            indices[ offset ] = 0;
            indices[ offset + 1 ] = ( uint )( i + 1 );
            indices[ offset + 2 ] = ( uint )( i + 2 );
        }

        return indices;
    }

    public static QuadVertex[] CreateRectangle( Vector3 position, float width, float height, Vector4 color )
    {
        float halfW = width / 2f;
        float halfH = height / 2f;

        return
        [
            new QuadVertex( position + new Vector3( -halfW, -halfH, 0 ), new Vector2( 0, 0 ), color ),
            new QuadVertex( position + new Vector3( halfW, -halfH, 0 ), new Vector2( 1, 0 ), color ),
            new QuadVertex( position + new Vector3( halfW, halfH, 0 ), new Vector2( 1, 1 ), color ),
            new QuadVertex( position + new Vector3( -halfW, halfH, 0 ), new Vector2( 0, 1 ), color )
        ];
    }

    public static QuadVertex[] CreateLine( Vector3 start, Vector3 end, float thickness, Vector4 color )
    {
        Vector3 direction = Vector3.Normalize( end - start );
        Vector3 perpendicular = new( -direction.Y, direction.X, 0 );
        Vector3 offset = perpendicular * thickness * 0.5f;

        return
        [
            new QuadVertex( start - offset, new Vector2( 0, 0 ), color ),
            new QuadVertex( start + offset, new Vector2( 1, 0 ), color ),
            new QuadVertex( end + offset, new Vector2( 1, 1 ), color ),
            new QuadVertex( end - offset, new Vector2( 0, 1 ), color )
        ];
    }
}