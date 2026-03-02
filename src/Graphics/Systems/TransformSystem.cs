using Aether.Core.Extensions;
using Graphics.Components;
using Silk.NET.Maths;

namespace Graphics.Systems;

public class TransformSystem
{
    public static Matrix4X4<float> GetWorldMatrix( ref Transform transform )
    {
        if ( !transform.IsDirty )
            return transform.CachedMatrix;

        transform.CachedMatrix =
            Matrix4X4.CreateScale( transform.Scale ) *
            Matrix4X4.CreateFromQuaternion( transform.Rotation ) *
            Matrix4X4.CreateTranslation( transform.Position );

        transform.IsDirty = false;
        return transform.CachedMatrix;
    }

    public static Matrix4X4<float> CreateModelMatrix( Vector3D<float> position, Quaternion<float> rotation,
        Vector3D<float> scale )
    {
        return Matrix4X4.CreateScale( scale ) *
               Matrix4X4.CreateFromQuaternion( rotation ) *
               Matrix4X4.CreateTranslation( position );
    }

    // Применяет трансформацию к локальным координатам и переводит в глобальные
    public static Vector2D<float>[] GetTransformedPolygon( Vector2D<float>[] localVertices, Transform transform )
    {
        Vector2D<float>[] transformed = new Vector2D<float>[ localVertices.Length ];

        float rotation = MathExtensions.GetAngleFromQuaternion( transform.Rotation );

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
}