using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Aether.Core.Extensions;
using Graphics.Components;

namespace Graphics.Systems;

public class CameraSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        foreach ( Entity entity in World.Filter<Camera>().With<Transform>() )
        {
            ref Camera camera = ref World.Get<Camera>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            // Only update rotation for perspective cameras
            if ( camera.ProjectionType == ProjectionType.Perspective )
            {
                UpdateCameraRotation( ref camera, ref transform );
            }

            Vector3D<float> cameraPosition = camera.IsStatic ? camera.StaticPosition : transform.Position;

            if ( camera.ProjectionType == ProjectionType.Perspective )
            {
                camera.ViewMatrix = TransformSystem.CreateViewMatrix(
                    cameraPosition,
                    cameraPosition + transform.Forward,
                    transform.Up
                );

                camera.ProjectionMatrix = TransformSystem.CreatePerspectiveProjection(
                    MathExtensions.DegToRad( camera.FieldOfView ),
                    camera.AspectRatio,
                    camera.NearPlane,
                    camera.FarPlane
                );
            }
            else
            {
                camera.ViewMatrix = Matrix4X4<float>.Identity;

                float halfWidth = camera.OrthographicSize * camera.AspectRatio;
                float halfHeight = camera.OrthographicSize;

                camera.ProjectionMatrix = TransformSystem.CreateOrthographicProjection(
                    cameraPosition.X - halfWidth,
                    cameraPosition.X + halfWidth,
                    cameraPosition.Y - halfHeight,
                    cameraPosition.Y + halfHeight,
                    camera.NearPlane,
                    camera.FarPlane
                );
            }

            camera.ViewProjectionMatrix = camera.ProjectionMatrix * camera.ViewMatrix;
        }
    }

    public void ProcessKeyboard( ref Camera camera, ref Transform transform, MovementType direction, float deltaTime )
    {
        float velocity = camera.MovementSpeed * deltaTime;
        Vector3D<float> movement = Vector3D<float>.Zero;

        switch ( direction )
        {
            case MovementType.Forward:
                movement = transform.Forward * velocity;
                break;
            case MovementType.Backward:
                movement = -transform.Forward * velocity;
                break;
            case MovementType.Left:
                movement = -transform.Right * velocity;
                break;
            case MovementType.Right:
                movement = transform.Right * velocity;
                break;
            case MovementType.Up:
                movement = transform.Up * velocity;
                break;
            case MovementType.Down:
                movement = -transform.Up * velocity;
                break;
        }

        if ( camera.IsStatic )
        {
            camera.StaticPosition += movement;
        }
        else
        {
            transform.Position += movement;
        }
    }

    public void ProcessMouseMovement( ref Camera camera, ref Transform transform, float xOffset, float yOffset,
        bool constrainPitch = true )
    {
        xOffset *= camera.MouseSensitivity;
        yOffset *= camera.MouseSensitivity;

        camera.Yaw += xOffset;
        camera.Pitch += yOffset;

        if ( constrainPitch )
        {
            camera.Pitch = Math.Clamp( camera.Pitch, -89.0f, 89.0f );
        }

        UpdateCameraRotation( ref camera, ref transform );
    }

    public void ProcessMouseScroll( ref Camera camera, float yOffset )
    {
        if ( camera.ProjectionType == ProjectionType.Perspective )
        {
            camera.FieldOfView -= yOffset * camera.ZoomSpeed;
            camera.FieldOfView = Math.Clamp( camera.FieldOfView, 1.0f, 45.0f );
        }
        else
        {
            camera.OrthographicSize -= yOffset * camera.ZoomSpeed;
            camera.OrthographicSize = Math.Max( camera.OrthographicSize, 0.1f );
        }
    }

    private static void UpdateCameraRotation( ref Camera camera, ref Transform transform )
    {
        float yawRad = MathExtensions.DegToRad( camera.Yaw );
        float pitchRad = MathExtensions.DegToRad( camera.Pitch );

        Quaternion<float> yawRotation = Quaternion<float>.CreateFromAxisAngle( Vector3D<float>.UnitY, yawRad );
        Quaternion<float> pitchRotation = Quaternion<float>.CreateFromAxisAngle( Vector3D<float>.UnitX, pitchRad );

        transform.Rotation = yawRotation * pitchRotation;
    }
}