using Silk.NET.Maths;

namespace Asteroids.Builders;

public static class EntityBuilder
{
    public static Vector2D<float>[] CreateAsteroidVertices( int seed )
    {
        Random random = new( seed );
        const int sides = 8;
        Vector2D<float>[] vertices = new Vector2D<float>[ sides ];

        for ( int i = 0; i < sides; i++ )
        {
            // Переходим в полярную систему координат -> 0/8, 1/8, 2/8 итд
            float angle = ( float )i / sides * MathF.PI * 2;
            // Радиус ограничен 0.4f до 0.6f
            float radius = 0.4f + ( float )random.NextDouble() * 0.2f;

            vertices[ i ] = new Vector2D<float>(
                MathF.Cos( angle ) * radius,
                MathF.Sin( angle ) * radius
            );
        }

        return vertices;
    }

    public static Vector2D<float>[] CreateSpaceshipVertices()
    {
        return
        [
            new Vector2D<float>( 0.5f, 0f ),
            new Vector2D<float>( -0.3f, 0.3f ),
            new Vector2D<float>( -0.2f, 0f ),
            new Vector2D<float>( -0.3f, -0.3f )
        ];
    }
}