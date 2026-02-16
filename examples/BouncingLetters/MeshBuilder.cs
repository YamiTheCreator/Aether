using System.Numerics;
using Aether.Core.Structures;
using Graphics;

namespace BouncingLetters;

public static class MeshBuilder
{
    public static (QuadVertex[] vertices, uint[] indices) CreateLetterC()
    {
        Vector2[] contour =
        [
            new( 0.1f, 0f ),
            new( 0.1f, 0.5f ),
            new( 0.2f, 0.5f ),
            new( 0.4f, 0.5f ),
            new( 0.4f, 0.4f ),
            new( 0.2f, 0.4f ),
            new( 0.2f, 0.1f ),
            new( 0.4f, 0.1f ),
            new( 0.4f, 0f ),
            new( 0.2f, 0f )
        ];

        QuadVertex[] vertices = contour.Select( p =>
            new QuadVertex( new Vector3( p.X, p.Y, 0 ), Vector2.Zero, Vector4.One, 0 ) ).ToArray();

        uint[] indices =
        [
            0, 1, 9,
            1, 9, 2,
            2, 3, 4,
            4, 5, 2,
            6, 7, 8,
            8, 9, 6
        ];

        return ( vertices, indices );
    }

    public static (QuadVertex[] vertices, uint[] indices) CreateLetterK()
    {
        Vector2[] contour =
        [
            new( 0.1f, 0f ),
            new( 0.1f, 0.5f ),
            new( 0.2f, 0.5f ),
            new( 0.2f, 0.3f ),
            new( 0.3f, 0.5f ),
            new( 0.4f, 0.5f ),
            new( 0.3f, 0.25f ),
            new( 0.4f, 0f ),
            new( 0.3f, 0f ),
            new( 0.2f, 0.2f ),
            new( 0.2f, 0f )
        ];

        QuadVertex[] vertices = contour.Select( p =>
            new QuadVertex( new Vector3( p.X, p.Y, 0 ), Vector2.Zero, Vector4.One, 0 ) ).ToArray();

        uint[] indices =
        [
            0, 1, 10,
            1, 2, 10,
            3, 4, 5,
            5, 6, 3,
            3, 6, 9,
            6, 7, 9,
            7, 8, 9
        ];

        return ( vertices, indices );
    }

    public static (QuadVertex[] vertices, uint[] indices) CreateLetterA()
    {
        Vector2[] contour =
        [
            new( 0f, 0f ),
            new( 0.2f, 0.5f ),
            new( 0.3f, 0.5f ),
            new( 0.5f, 0f ),
            new( 0.4f, 0f ),
            new( 0.25f, 0.1f ),
            new( 0.1f, 0f ),
            new( 0.2f, 0.2f ),
            new( 0.25f, 0.4f ),
            new( 0.3f, 0.2f )
        ];

        QuadVertex[] vertices = contour.Select( p =>
            new QuadVertex( new Vector3( p.X, p.Y, 0 ), Vector2.Zero, Vector4.One, 0 ) ).ToArray();

        uint[] indices =
        [
            0, 1, 6,
            1, 2, 6,
            2, 3, 4,
            4, 2, 8,
            5, 7, 9
        ];

        return ( vertices, indices );
    }
}