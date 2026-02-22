using System.Runtime.InteropServices;
using Aether.Core;
using Aether.Core.Enums;
using Graphics.Components;
using Graphics.Structures;
using Silk.NET.Maths;

namespace Graphics.Systems;

public class CameraSystem : SystemBase
{
    private const float _defaultMovementSpeed = 5.0f;
    private const float _defaultMouseSensitivity = 0.1f;

    public float MovementSpeed { get; set; } = _defaultMovementSpeed;
    public float MouseSensitivity { get; set; } = _defaultMouseSensitivity;

    protected override void OnInit()
    {
    }

    protected override void OnUpdate( float deltaTime )
    {
        UpdateCameraMatrices();
    }

    private void UpdateCameraMatrices()
    {
        foreach ( Entity entity in World.Filter<Camera>() )
        {
            ref Camera camera = ref World.Get<Camera>( entity );

            if ( camera.IsOrbitMode )
            {
                Vector3D<float> orbitPosition = camera.StaticPosition;
                camera.ViewMatrix = CalculateViewMatrix( orbitPosition, camera.Forward, camera.Up );
                camera.ProjectionMatrix = CalculateProjectionMatrix( ref camera );
                camera.ViewProjectionMatrix = camera.ViewMatrix * camera.ProjectionMatrix;
                continue;
            }

            if ( camera.ProjectionType == ProjectionType.Perspective )
            {
                UpdateCameraVectors( ref camera );
            }
            else
            {
                if ( camera.Forward == Vector3D<float>.Zero )
                {
                    camera.Forward = new Vector3D<float>( 0, 0, -1 );
                    camera.Right = new Vector3D<float>( 1, 0, 0 );
                    camera.Up = new Vector3D<float>( 0, 1, 0 );
                }
            }

            Vector3D<float> position = camera.IsStatic
                ? camera.StaticPosition
                : World.Get<Transform>( entity ).Position;

            camera.ViewMatrix = CalculateViewMatrix( position, camera.Forward, camera.Up );
            camera.ProjectionMatrix = CalculateProjectionMatrix( ref camera );
            camera.ViewProjectionMatrix = camera.ViewMatrix * camera.ProjectionMatrix;
        }
    }

    private Matrix4X4<float> CalculateViewMatrix( Vector3D<float> position, Vector3D<float> forward,
        Vector3D<float> up )
    {
        Vector3D<float> target = position + forward;
        return Matrix4X4.CreateLookAt( position, target, up );
    }

    private Matrix4X4<float> CalculateProjectionMatrix( ref Camera camera )
    {
        if ( camera.ProjectionType == ProjectionType.Perspective )
        {
            return Matrix4X4.CreatePerspectiveFieldOfView(
                camera.FieldOfView * ( MathF.PI / 180.0f ),
                camera.AspectRatio,
                camera.NearPlane,
                camera.FarPlane
            );
        }
        else
        {
            float width = camera.OrthographicSize * camera.AspectRatio;
            float height = camera.OrthographicSize;
            return Matrix4X4.CreateOrthographic(
                width,
                height,
                camera.NearPlane,
                camera.FarPlane
            );
        }
    }

    public void ProcessKeyboard( ref Camera camera, ref Transform transform, MovementType direction, float deltaTime )
    {
        float velocity = MovementSpeed * deltaTime;

        Vector3D<float> forward = camera.Forward;
        Vector3D<float> right = camera.Right;
        Vector3D<float> up = camera.Up;

        switch ( direction )
        {
            case MovementType.Forward:
                transform.Position += forward * velocity;
                break;
            case MovementType.Backward:
                transform.Position -= forward * velocity;
                break;
            case MovementType.Left:
                transform.Position -= right * velocity;
                break;
            case MovementType.Right:
                transform.Position += right * velocity;
                break;
            case MovementType.Up:
                transform.Position += up * velocity;
                break;
            case MovementType.Down:
                transform.Position -= up * velocity;
                break;
        }
    }

    public void ProcessMouseMovement( ref Camera camera, ref Transform transform, float xOffset, float yOffset,
        bool constrainPitch = true )
    {
        xOffset *= -MouseSensitivity;
        yOffset *= MouseSensitivity;

        camera.Yaw += xOffset;
        camera.Pitch += yOffset;

        if ( constrainPitch )
        {
            if ( camera.Pitch > 89.0f )
                camera.Pitch = 89.0f;
            if ( camera.Pitch < -89.0f )
                camera.Pitch = -89.0f;
        }

        UpdateCameraVectors( ref camera );
    }

    private void UpdateCameraVectors( ref Camera camera )
    {
        float yawRad = camera.Yaw * ( MathF.PI / 180.0f );
        float pitchRad = camera.Pitch * ( MathF.PI / 180.0f );

        Vector3D<float> forward;
        forward.X = MathF.Cos( yawRad ) * MathF.Cos( pitchRad );
        forward.Y = MathF.Sin( pitchRad );
        forward.Z = MathF.Sin( yawRad ) * MathF.Cos( pitchRad );

        camera.Forward = Vector3D.Normalize( forward );
        camera.Right = Vector3D.Normalize( Vector3D.Cross( camera.Forward, Vector3D<float>.UnitY ) );
        camera.Up = Vector3D.Normalize( Vector3D.Cross( camera.Right, camera.Forward ) );
    }
}