using Graphics.Components;
using Silk.NET.Maths;

namespace Graphics.Systems;

public static class CollisionSystem
{
    public static bool GJK( Vector2D<float>[] shapeA, Vector2D<float>[] shapeB )
    {
        Vector2D<float> direction = new( 1, 0 );
        List<Vector2D<float>> simplex = InitializeSimplex( shapeA, shapeB, ref direction );

        const int maxIterations = 32;
        return IterateGJK( shapeA, shapeB, simplex, ref direction, maxIterations );
    }

    private static List<Vector2D<float>> InitializeSimplex( Vector2D<float>[] shapeA, Vector2D<float>[] shapeB,
        ref Vector2D<float> direction )
    {
        List<Vector2D<float>> simplex = new();
        Vector2D<float> support = GetSupport( shapeA, shapeB, direction );
        simplex.Add( support );
        direction = -support;
        return simplex;
    }

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

    private static bool IsSupportValid( Vector2D<float> support, Vector2D<float> direction )
    {
        return Vector2D.Dot( support, direction ) > 0;
    }

    private static Vector2D<float> GetSupport( Vector2D<float>[] shapeA, Vector2D<float>[] shapeB,
        Vector2D<float> direction )
    {
        Vector2D<float> pointA = GetFarthestPointInDirection( shapeA, direction );
        Vector2D<float> pointB = GetFarthestPointInDirection( shapeB, -direction );
        return pointA - pointB;
    }

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

    private static bool ProcessSimplex( List<Vector2D<float>> simplex, ref Vector2D<float> direction )
    {
        if ( simplex.Count == 2 )
        {
            return ProcessLine( simplex, ref direction );
        }

        return ProcessTriangle( simplex, ref direction );
    }

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

    private static Vector2D<float> TripleProduct( Vector2D<float> a, Vector2D<float> b, Vector2D<float> c )
    {
        float ac = a.X * c.X + a.Y * c.Y;
        float bc = b.X * c.X + b.Y * c.Y;
        return new Vector2D<float>( b.X * ac - a.X * bc, b.Y * ac - a.Y * bc );
    }

    public static Vector2D<float>[] GetTransformedPolygon( Vector2D<float>[] localVertices, Transform transform )
    {
        Vector2D<float>[] transformed = new Vector2D<float>[ localVertices.Length ];

        float rotation = GetAngleFromQuaternion( transform.Rotation );

        float cos = MathF.Cos( rotation );
        float sin = MathF.Sin( rotation );

        for ( int i = 0; i < localVertices.Length; i++ )
        {
            float x = localVertices[ i ].X * transform.Scale.X;
            float y = localVertices[ i ].Y * transform.Scale.Y;

            float rotatedX = x * cos - y * sin;
            float rotatedY = x * sin + y * cos;

            transformed[ i ] = new Vector2D<float>(
                rotatedX + transform.Position.X,
                rotatedY + transform.Position.Y
            );
        }

        return transformed;
    }

    private static float GetAngleFromQuaternion( Quaternion<float> rotation )
    {
        return 2f * MathF.Atan2( rotation.Z, rotation.W );
    }

    public static bool CheckCircleGridCollision<T>( T grid, float worldX, float worldZ, float radius,
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
                if ( x < 0 || x >= gridWidth || z < 0 || z >= gridHeight )
                    return true;

                if ( isWallFunc( grid, x, z ) )
                {
                    float closestX = Math.Clamp( worldX, x, x + 1 );
                    float closestZ = Math.Clamp( worldZ, z, z + 1 );

                    float dx = worldX - closestX;
                    float dz = worldZ - closestZ;
                    float distSq = dx * dx + dz * dz;

                    if ( distSq < radius * radius )
                        return true;
                }
            }
        }

        return false;
    }

    public static bool GJK3D( Vector3D<float>[] shapeA, Vector3D<float>[] shapeB )
    {
        Vector3D<float> direction = new( 1, 0, 0 );
        List<Vector3D<float>> simplex = InitializeSimplex3D( shapeA, shapeB, ref direction );

        const int maxIterations = 64;
        return IterateGJK3D( shapeA, shapeB, simplex, ref direction, maxIterations );
    }

    private static List<Vector3D<float>> InitializeSimplex3D( Vector3D<float>[] shapeA, Vector3D<float>[] shapeB,
        ref Vector3D<float> direction )
    {
        List<Vector3D<float>> simplex = new();
        Vector3D<float> support = GetSupport3D( shapeA, shapeB, direction );
        simplex.Add( support );
        direction = -support;
        return simplex;
    }

    private static bool IterateGJK3D( Vector3D<float>[] shapeA, Vector3D<float>[] shapeB,
        List<Vector3D<float>> simplex, ref Vector3D<float> direction, int maxIterations )
    {
        for ( int i = 0; i < maxIterations; i++ )
        {
            Vector3D<float> support = GetSupport3D( shapeA, shapeB, direction );

            if ( !IsSupportValid3D( support, direction ) )
            {
                return false;
            }

            simplex.Add( support );

            if ( ProcessSimplex3D( simplex, ref direction ) )
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSupportValid3D( Vector3D<float> support, Vector3D<float> direction )
    {
        return Vector3D.Dot( support, direction ) > 0;
    }

    private static Vector3D<float> GetSupport3D( Vector3D<float>[] shapeA, Vector3D<float>[] shapeB,
        Vector3D<float> direction )
    {
        Vector3D<float> pointA = GetFarthestPointInDirection3D( shapeA, direction );
        Vector3D<float> pointB = GetFarthestPointInDirection3D( shapeB, -direction );
        return pointA - pointB;
    }

    private static Vector3D<float> GetFarthestPointInDirection3D( Vector3D<float>[] shape, Vector3D<float> direction )
    {
        float maxDot = float.MinValue;
        Vector3D<float> farthest = shape[ 0 ];

        foreach ( Vector3D<float> vertex in shape )
        {
            float dot = Vector3D.Dot( vertex, direction );
            if ( dot > maxDot )
            {
                maxDot = dot;
                farthest = vertex;
            }
        }

        return farthest;
    }

    private static bool ProcessSimplex3D( List<Vector3D<float>> simplex, ref Vector3D<float> direction )
    {
        switch ( simplex.Count )
        {
            case 2:
                return ProcessLine3D( simplex, ref direction );
            case 3:
                return ProcessTriangle3D( simplex, ref direction );
            case 4:
                return ProcessTetrahedron( simplex, ref direction );
            default:
                return false;
        }
    }

    private static bool ProcessLine3D( List<Vector3D<float>> simplex, ref Vector3D<float> direction )
    {
        Vector3D<float> a = simplex[ 1 ];
        Vector3D<float> b = simplex[ 0 ];
        Vector3D<float> ab = b - a;
        Vector3D<float> ao = -a;

        if ( Vector3D.Dot( ab, ao ) > 0 )
        {
            direction = Vector3D.Cross( Vector3D.Cross( ab, ao ), ab );
        }
        else
        {
            simplex.RemoveAt( 0 );
            direction = ao;
        }

        return false;
    }

    private static bool ProcessTriangle3D( List<Vector3D<float>> simplex, ref Vector3D<float> direction )
    {
        Vector3D<float> a = simplex[ 2 ];
        Vector3D<float> b = simplex[ 1 ];
        Vector3D<float> c = simplex[ 0 ];

        Vector3D<float> ab = b - a;
        Vector3D<float> ac = c - a;
        Vector3D<float> ao = -a;

        Vector3D<float> abc = Vector3D.Cross( ab, ac );

        if ( Vector3D.Dot( Vector3D.Cross( abc, ac ), ao ) > 0 )
        {
            if ( Vector3D.Dot( ac, ao ) > 0 )
            {
                simplex.RemoveAt( 1 );
                direction = Vector3D.Cross( Vector3D.Cross( ac, ao ), ac );
            }
            else
            {
                simplex.RemoveAt( 0 );
                simplex.RemoveAt( 0 );
                direction = Vector3D.Dot( ab, ao ) > 0
                    ? Vector3D.Cross( Vector3D.Cross( ab, ao ), ab )
                    : ao;
            }

            return false;
        }

        if ( Vector3D.Dot( Vector3D.Cross( ab, abc ), ao ) > 0 )
        {
            simplex.RemoveAt( 0 );
            direction = Vector3D.Dot( ab, ao ) > 0
                ? Vector3D.Cross( Vector3D.Cross( ab, ao ), ab )
                : ao;
            return false;
        }

        if ( Vector3D.Dot( abc, ao ) > 0 )
        {
            direction = abc;
        }
        else
        {
            ( simplex[ 0 ], simplex[ 1 ] ) = ( simplex[ 1 ], simplex[ 0 ] );
            direction = -abc;
        }

        return false;
    }

    private static bool ProcessTetrahedron( List<Vector3D<float>> simplex, ref Vector3D<float> direction )
    {
        Vector3D<float> a = simplex[ 3 ];
        Vector3D<float> b = simplex[ 2 ];
        Vector3D<float> c = simplex[ 1 ];
        Vector3D<float> d = simplex[ 0 ];

        Vector3D<float> ab = b - a;
        Vector3D<float> ac = c - a;
        Vector3D<float> ad = d - a;
        Vector3D<float> ao = -a;

        Vector3D<float> abc = Vector3D.Cross( ab, ac );
        Vector3D<float> acd = Vector3D.Cross( ac, ad );
        Vector3D<float> adb = Vector3D.Cross( ad, ab );

        if ( Vector3D.Dot( abc, ao ) > 0 )
        {
            simplex.RemoveAt( 0 );
            return ProcessTriangle3D( simplex, ref direction );
        }

        if ( Vector3D.Dot( acd, ao ) > 0 )
        {
            simplex.RemoveAt( 2 );
            return ProcessTriangle3D( simplex, ref direction );
        }

        if ( Vector3D.Dot( adb, ao ) > 0 )
        {
            simplex.RemoveAt( 1 );
            return ProcessTriangle3D( simplex, ref direction );
        }

        return true;
    }

    public static Vector3D<float>[] GetTransformedVertices3D( Vector3D<float>[] localVertices, Transform transform )
    {
        Vector3D<float>[] transformed = new Vector3D<float>[ localVertices.Length ];

        for ( int i = 0; i < localVertices.Length; i++ )
        {
            Vector3D<float> scaled = localVertices[ i ] * transform.Scale;
            Vector3D<float> rotated = Vector3D.Transform( scaled, transform.Rotation );
            transformed[ i ] = rotated + transform.Position;
        }

        return transformed;
    }
}
