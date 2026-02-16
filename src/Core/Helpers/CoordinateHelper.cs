using System.Numerics;

namespace Aether.Core.Helpers;

public static class CoordinateHelper
{
    public static Matrix4x4 WorldToView( Vector3 cameraPosition, Vector3 target, Vector3 up ) =>
        Matrix4x4.CreateLookAt( cameraPosition, target, up );

    public static Matrix4x4 ViewToClipPerspective( float fieldOfView, float aspectRatio, float nearPlane,
        float farPlane ) =>
        Matrix4x4.CreatePerspectiveFieldOfView( fieldOfView, aspectRatio, nearPlane, farPlane );

    public static Matrix4x4 ViewToClipOrthographic( float left, float right, float bottom, float top, float nearPlane,
        float farPlane ) =>
        Matrix4x4.CreateOrthographicOffCenter( left, right, bottom, top, nearPlane, farPlane );

    public static Vector3 ScreenToWorld( Vector2 screenPoint, float screenWidth, float screenHeight,
        Matrix4x4 viewProjection, float depth = 0f )
    {
        float ndcX = screenPoint.X / screenWidth * 2.0f - 1.0f;
        float ndcY = 1.0f - screenPoint.Y / screenHeight * 2.0f;
        float ndcZ = depth * 2.0f - 1.0f;

        Vector4 clipSpace = new( ndcX, ndcY, ndcZ, 1.0f );

        Matrix4x4.Invert( viewProjection, out Matrix4x4 invViewProjection );

        Vector4 worldSpace = Vector4.Transform( clipSpace, invViewProjection );

        if ( worldSpace.W != 0 )
        {
            worldSpace /= worldSpace.W;
        }

        return new Vector3( worldSpace.X, worldSpace.Y, worldSpace.Z );
    }
}