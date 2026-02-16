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

            // Update camera vectors based on yaw and pitch
            UpdateCameraVectors( ref camera );

            // Determine camera position (static or from transform)
            Vector3 cameraPosition = camera.IsStatic ? camera.StaticPosition : transform.Position;

            // Update projection matrix
            if ( camera.ProjectionType == ProjectionType.Perspective )
            {
                // 3D Perspective: use LookAt view matrix
                camera.ViewMatrix = CoordinateHelper.WorldToView(
                    cameraPosition,
                    cameraPosition + camera.Front,
                    camera.Up
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
                movement = camera.Front * velocity;
                break;
            case CameraMovement.Backward:
                movement = -camera.Front * velocity;
                break;
            case CameraMovement.Left:
                movement = -camera.Right * velocity;
                break;
            case CameraMovement.Right:
                movement = camera.Right * velocity;
                break;
            case CameraMovement.Up:
                movement = camera.Up * velocity;
                break;
            case CameraMovement.Down:
                movement = -camera.Up * velocity;
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

    public void ProcessMouseMovement( ref Camera camera, float xOffset, float yOffset, bool constrainPitch = true )
    {
        xOffset *= camera.MouseSensitivity;
        yOffset *= camera.MouseSensitivity;

        camera.Yaw += xOffset;
        camera.Pitch += yOffset;

        if ( constrainPitch )
        {
            camera.Pitch = Math.Clamp( camera.Pitch, -89.0f, 89.0f );
        }

        UpdateCameraVectors( ref camera );
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

    private static void UpdateCameraVectors( ref Camera camera )
    {
        float yawRad = MathExtensions.DegToRad( camera.Yaw );
        float pitchRad = MathExtensions.DegToRad( camera.Pitch );

        Vector3 front;
        front.X = ( float )( Math.Cos( yawRad ) * Math.Cos( pitchRad ) );
        front.Y = ( float )Math.Sin( pitchRad );
        front.Z = ( float )( Math.Sin( yawRad ) * Math.Cos( pitchRad ) );

        camera.Front = Vector3.Normalize( front );
        camera.Right = Vector3.Normalize( Vector3.Cross( camera.Front, camera.WorldUp ) );
        camera.Up = Vector3.Normalize( Vector3.Cross( camera.Right, camera.Front ) );
    }
}