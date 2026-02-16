using System.Numerics;
using Aether.Core;
using Aether.Core.Enums;
using Aether.Core.Extensions;
using Aether.Core.Helpers;
using Aether.Core.Systems;
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

            // Update transform rotation based on yaw and pitch
            UpdateCameraRotation( ref camera, ref transform );

            // Determine camera position (static or from transform)
            Vector3 cameraPosition = camera.IsStatic ? camera.StaticPosition : transform.Position;

            // Update projection matrix
            if ( camera.ProjectionType == ProjectionType.Perspective )
            {
                // 3D Perspective: use LookAt view matrix with Transform vectors
                camera.ViewMatrix = CoordinateHelper.WorldToView(
                    cameraPosition,
                    cameraPosition + transform.Forward,
                    transform.Up
                );

                camera.ProjectionMatrix = CoordinateHelper.ViewToClipPerspective(
                    MathExtensions.DegToRad( camera.FieldOfView ),
                    camera.AspectRatio,
                    camera.NearPlane,
                    camera.FarPlane
                );
            }
            else
            {
                // 2D Orthographic: use identity view
                camera.ViewMatrix = Matrix4x4.Identity;

                // Use OrthographicSize centered on camera position
                float halfWidth = camera.OrthographicSize * camera.AspectRatio;
                float halfHeight = camera.OrthographicSize;

                camera.ProjectionMatrix = CoordinateHelper.ViewToClipOrthographic(
                    cameraPosition.X - halfWidth,
                    cameraPosition.X + halfWidth,
                    cameraPosition.Y - halfHeight,
                    cameraPosition.Y + halfHeight,
                    camera.NearPlane,
                    camera.FarPlane
                );
            }

            // Update combined view-projection matrix
            camera.ViewProjectionMatrix = camera.ProjectionMatrix * camera.ViewMatrix;
        }
    }

    public void ProcessKeyboard( ref Camera camera, ref Transform transform, CameraMovement direction, float deltaTime )
    {
        float velocity = camera.MovementSpeed * deltaTime;
        Vector3 movement = Vector3.Zero;

        switch ( direction )
        {
            case CameraMovement.Forward:
                movement = transform.Forward * velocity;
                break;
            case CameraMovement.Backward:
                movement = -transform.Forward * velocity;
                break;
            case CameraMovement.Left:
                movement = -transform.Right * velocity;
                break;
            case CameraMovement.Right:
                movement = transform.Right * velocity;
                break;
            case CameraMovement.Up:
                movement = transform.Up * velocity;
                break;
            case CameraMovement.Down:
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

    public void ProcessMouseMovement( ref Camera camera, ref Transform transform, float xOffset, float yOffset, bool constrainPitch = true )
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

        // Create rotation quaternion from yaw and pitch
        Quaternion yawRotation = Quaternion.CreateFromAxisAngle( Vector3.UnitY, yawRad );
        Quaternion pitchRotation = Quaternion.CreateFromAxisAngle( Vector3.UnitX, pitchRad );
        
        transform.Rotation = yawRotation * pitchRotation;
    }
}