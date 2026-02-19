using Graphics.Structures;
using Silk.NET.Maths;

namespace Graphics.Builders;

public static class PrimitivesBuilder
{
    public static Vertex[] CreateCircle( Vector3D<float> center, float radius, Vector4D<float> color,
        int segments = 32 )
    {
        int vertexCount = segments + 2;
        Vertex[] vertices = new Vertex[ vertexCount ];

        vertices[ 0 ] = new Vertex( center, new Vector2D<float>( 0.5f, 0.5f ), color );

        for ( int i = 0; i <= segments; i++ )
        {
            float angle = ( float )i / segments * MathF.PI * 2f;
            float x = center.X + MathF.Cos( angle ) * radius;
            float y = center.Y + MathF.Sin( angle ) * radius;
            float u = ( MathF.Cos( angle ) + 1f ) * 0.5f;
            float v = ( MathF.Sin( angle ) + 1f ) * 0.5f;

            vertices[ i + 1 ] = new Vertex( new Vector3D<float>( x, y, center.Z ), new Vector2D<float>( u, v ), color );
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

    public static Vertex[] CreateRectangle( Vector3D<float> position, float width, float height, Vector4D<float> color )
    {
        float halfW = width / 2f;
        float halfH = height / 2f;

        return
        [
            new Vertex( position + new Vector3D<float>( -halfW, -halfH, 0 ), new Vector2D<float>( 0, 0 ), color ),
            new Vertex( position + new Vector3D<float>( halfW, -halfH, 0 ), new Vector2D<float>( 1, 0 ), color ),
            new Vertex( position + new Vector3D<float>( halfW, halfH, 0 ), new Vector2D<float>( 1, 1 ), color ),
            new Vertex( position + new Vector3D<float>( -halfW, halfH, 0 ), new Vector2D<float>( 0, 1 ), color )
        ];
    }

    public static Vertex[] CreateLine( Vector3D<float> start, Vector3D<float> end, float thickness,
        Vector4D<float> color )
    {
        Vector3D<float> direction = Vector3D.Normalize( end - start );
        Vector3D<float> perpendicular = new( -direction.Y, direction.X, 0 );
        Vector3D<float> offset = perpendicular * thickness * 0.5f;

        return
        [
            new Vertex( start - offset, new Vector2D<float>( 0, 0 ), color ),
            new Vertex( start + offset, new Vector2D<float>( 1, 0 ), color ),
            new Vertex( end + offset, new Vector2D<float>( 1, 1 ), color ),
            new Vertex( end - offset, new Vector2D<float>( 0, 1 ), color )
        ];
    }
}