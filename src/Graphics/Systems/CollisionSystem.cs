using Silk.NET.Maths;

namespace Graphics.Systems;

public static class CollisionSystem
{
    // Алгоритм GJK (Gilbert-Johnson-Keerthi) для определения пересечения двух выпуклых фигур в 2D
    // Работает через построение симплекса в пространстве разности Минковского
    public static bool Gjk( Vector2D<float>[] shapeA, Vector2D<float>[] shapeB )
    {
        Vector2D<float> direction = new( 1, 0 );
        List<Vector2D<float>> simplex = InitializeSimplex( shapeA, shapeB, ref direction );

        const int maxIterations = 32;
        return IterateGJK( shapeA, shapeB, simplex, ref direction, maxIterations );
    }

    // Симплекс - набор опорных точек, в 2D не более 3х
    private static List<Vector2D<float>> InitializeSimplex( Vector2D<float>[] shapeA, Vector2D<float>[] shapeB,
        ref Vector2D<float> direction )
    {
        List<Vector2D<float>> simplex = [ ];
        Vector2D<float> support = GetSupport( shapeA, shapeB, direction );
        simplex.Add( support );
        direction = -support;
        return simplex;
    }
    
    // Запрашиваем точки разности, проверяя, находятся ли они ближе к началу координат.
    private static bool IterateGJK( Vector2D<float>[] shapeA, Vector2D<float>[] shapeB,
        List<Vector2D<float>> simplex, ref Vector2D<float> direction, int maxIterations )
    {
        for ( int i = 0; i < maxIterations; i++ )
        {
            Vector2D<float> support = GetSupport( shapeA, shapeB, direction );

            if ( !IsSupportValid( support, direction ) )
            {
                return false;
            }

            simplex.Add( support );

            if ( ProcessSimplex( simplex, ref direction ) )
            {
                return true;
            }
        }

        return false;
    }

    // Проверяем, что опорная точка находится в направлении поиска
    // Если нет - фигуры не пересекаются
    private static bool IsSupportValid( Vector2D<float> support, Vector2D<float> direction )
    {
        return Vector2D.Dot( support, direction ) > 0;
    }

    // Возвращаем точку в пространстве разности Минковского
    private static Vector2D<float> GetSupport( Vector2D<float>[] shapeA, Vector2D<float>[] shapeB,
        Vector2D<float> direction )
    {
        Vector2D<float> pointA = GetFarthestPointInDirection( shapeA, direction );
        Vector2D<float> pointB = GetFarthestPointInDirection( shapeB, -direction );
        return pointA - pointB;
    }

    //How to work Dot func
    // Для каждой точки берем скалярное произведение точки на направление,
    // у самой удаленной точки будет наибольшая проекция
    private static Vector2D<float> GetFarthestPointInDirection( Vector2D<float>[] shape, Vector2D<float> direction )
    {
        float maxDot = float.MinValue;
        Vector2D<float> farthest = shape[ 0 ];

        foreach ( Vector2D<float> vertex in shape )
        {
            float dot = Vector2D.Dot( vertex, direction );
            if ( dot > maxDot )
            {
                maxDot = dot;
                farthest = vertex;
            }
        }

        return farthest;
    }

    // Делегируем обработку симплекса в зависимости от количества точек
    // 2 точки - линия, 3 точки - треугольник
    private static bool ProcessSimplex( List<Vector2D<float>> simplex, ref Vector2D<float> direction )
    {
        if ( simplex.Count == 2 )
        {
            return ProcessLine( simplex, direction: ref direction );
        }

        return ProcessTriangle( simplex, ref direction );
    }

    // Process методы позволяют узнать с какой стороны находится начало координат и направить туда direction вектор
    // Делается это с помощью векторного произведения, которое позволяет нам получить новый вектор перпендикулярный прошлым
    private static bool ProcessLine( List<Vector2D<float>> simplex, ref Vector2D<float> direction )
    {
        Vector2D<float> a = simplex[ 1 ];
        Vector2D<float> b = simplex[ 0 ];
        Vector2D<float> ab = b - a;
        Vector2D<float> ao = -a;

        if ( Vector2D.Dot( ab, ao ) > 0 )
        {
            direction = TripleProduct( ab, ao, ab );
        }
        else
        {
            simplex.RemoveAt( 0 );
            direction = ao;
        }

        return false;
    }

    // Обрабатываем треугольник - проверяем с какой стороны от рёбер находится начало координат
    // Если начало внутри треугольника - коллизия найдена
    private static bool ProcessTriangle( List<Vector2D<float>> simplex, ref Vector2D<float> direction )
    {
        Vector2D<float> a = simplex[ 2 ];
        Vector2D<float> b = simplex[ 1 ];
        Vector2D<float> c = simplex[ 0 ];

        Vector2D<float> ab = b - a;
        Vector2D<float> ac = c - a;
        Vector2D<float> ao = -a;

        Vector2D<float> abPerp = TripleProduct( ac, ab, ab );
        Vector2D<float> acPerp = TripleProduct( ab, ac, ac );

        if ( Vector2D.Dot( abPerp, ao ) > 0 )
        {
            simplex.RemoveAt( 0 );
            direction = abPerp;
            return false;
        }

        if ( Vector2D.Dot( acPerp, ao ) > 0 )
        {
            simplex.RemoveAt( 1 );
            direction = acPerp;
            return false;
        }

        return true;
    }

    // векторное произведение -> дает вектор перпендикулярный a и направленный в сторону b
    private static Vector2D<float> TripleProduct( Vector2D<float> a, Vector2D<float> b, Vector2D<float> c )
    {
        float ac = a.X * c.X + a.Y * c.Y;
        float bc = b.X * c.X + b.Y * c.Y;
        return new Vector2D<float>( b.X * ac - a.X * bc, b.Y * ac - a.Y * bc );
    }                                         
}