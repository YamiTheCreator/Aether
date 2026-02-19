using Aether.Core.Extensions;
using Graphics.Components;
using Silk.NET.Maths;

namespace Graphics.Systems;

public class TransformSystem
{
    public Vector3D<float> SetPosition( ref Transform transform, Vector3D<float> position ) =>
        transform.Position = position;

    public Quaternion<float> SetRotation( ref Transform transform, Vector3D<float> rotation )
    {
        transform.IsDirty = true;

        return transform.Rotation = Quaternion<float>.CreateFromYawPitchRoll
        (
            MathExtensions.DegToRad( rotation.Y ),
            MathExtensions.DegToRad( rotation.X ),
            MathExtensions.DegToRad( rotation.Z )
        );
    }

    public Vector3D<float> SetScale( ref Transform transform, Vector3D<float> scale ) =>
        transform.Scale = scale;

    public Vector3D<float> SetForwardVector( ref Transform transform ) =>
        transform.Forward = Vector3D.Transform( -Vector3D<float>.UnitZ, transform.Rotation );

    public Vector3D<float> SetRightVector( ref Transform transform ) =>
        transform.Right = Vector3D.Transform( -Vector3D<float>.UnitX, transform.Rotation );

    public Vector3D<float> SetUpVector( ref Transform transform ) =>
        transform.Up = Vector3D.Transform( -Vector3D<float>.UnitY, transform.Rotation );

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

    public static Matrix4X4<float> GetLocalMatrix( ref Transform transform )
    {
        return GetWorldMatrix( ref transform );
    }

    public static Matrix4X4<float> CreateViewMatrix( Vector3D<float> cameraPosition, Vector3D<float> target,
        Vector3D<float> up )
    {
        return Matrix4X4.CreateLookAt( cameraPosition, target, up );
    }

    public static Matrix4X4<float> CreatePerspectiveProjection( float fieldOfView, float aspectRatio, float nearPlane,
        float farPlane )
    {
        return Matrix4X4.CreatePerspectiveFieldOfView( fieldOfView, aspectRatio, nearPlane, farPlane );
    }

    public static Matrix4X4<float> CreateOrthographicProjection( float left, float right, float bottom, float top,
        float nearPlane, float farPlane )
    {
        return Matrix4X4.CreateOrthographicOffCenter( left, right, bottom, top, nearPlane, farPlane );
    }

    public static Vector3D<float> ScreenToWorld( Vector2D<float> screenPoint, float screenWidth, float screenHeight,
        Matrix4X4<float> viewProjection, float depth = 0f )
    {
        float ndcX = screenPoint.X / screenWidth * 2.0f - 1.0f;
        float ndcY = 1.0f - screenPoint.Y / screenHeight * 2.0f;
        float ndcZ = depth * 2.0f - 1.0f;

        Vector4D<float> clipSpace = new( ndcX, ndcY, ndcZ, 1.0f );

        Matrix4X4.Invert( viewProjection, out Matrix4X4<float> invViewProjection );

        Vector4D<float> worldSpace = Vector4D.Transform( clipSpace, invViewProjection );

        if ( worldSpace.W != 0 )
        {
            worldSpace /= worldSpace.W;
        }

        return new Vector3D<float>( worldSpace.X, worldSpace.Y, worldSpace.Z );
    }

    public static Vector3D<float> ScreenToWorldOrthographic(
        Vector2D<float> screenPoint,
        float screenWidth,
        float screenHeight,
        Vector3D<float> cameraPosition,
        float orthographicSize,
        float aspectRatio )
    {
        float halfWidth = orthographicSize * aspectRatio;
        float halfHeight = orthographicSize;

        float normalizedX = screenPoint.X / screenWidth;
        float normalizedY = screenPoint.Y / screenHeight;

        return new Vector3D<float>(
            cameraPosition.X - halfWidth + normalizedX * ( halfWidth * 2 ),
            cameraPosition.Y + halfHeight - normalizedY * ( halfHeight * 2 ),
            cameraPosition.Z
        );
    }
}