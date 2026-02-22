using Aether.Core;
using Aether.Core.Enums;
using Silk.NET.Maths;

namespace Graphics.Components;

/// <summary>
/// Camera component - contains view/projection data and helper methods
/// </summary>
public struct Camera : Component
{
    public ProjectionType ProjectionType;

    // Perspective settings
    public float FieldOfView;
    public float AspectRatio;

    // Orthographic settings
    public float OrthographicSize;

    // Common settings
    public float NearPlane;
    public float FarPlane;

    // Camera orientation (for FPS-style control)
    public float Yaw;
    public float Pitch;

    // Camera vectors (calculated from Yaw/Pitch)
    public Vector3D<float> Forward;
    public Vector3D<float> Right;
    public Vector3D<float> Up;

    // Camera control settings
    public float MovementSpeed;
    public float MouseSensitivity;
    public float ZoomSpeed;

    // Static camera (doesn't use Transform position)
    public bool IsStatic;
    public Vector3D<float> StaticPosition;
    public Vector3D<float> WorldUp;

    // Orbit camera mode
    public bool IsOrbitMode;
    public Vector3D<float> OrbitTarget;
    public float OrbitDistance;
    public float OrbitYaw;
    public float OrbitPitch;
    public float OrbitMinDistance;
    public float OrbitMaxDistance;
    public float OrbitRotationSpeed;
    public float OrbitZoomSpeed;

    // Cached matrices (updated by CameraSystem)
    public Matrix4X4<float> ViewMatrix;
    public Matrix4X4<float> ProjectionMatrix;
    public Matrix4X4<float> ViewProjectionMatrix;

    public static Camera CreatePerspective(float fov = 45f, float aspectRatio = 16f / 9f)
    {
        return new Camera
        {
            ProjectionType = ProjectionType.Perspective,
            FieldOfView = fov,
            AspectRatio = aspectRatio,
            NearPlane = 0.1f,
            FarPlane = 100f,
            Yaw = -90f,
            Pitch = 0f,
            Forward = new Vector3D<float>(0, 0, -1),
            Right = new Vector3D<float>(1, 0, 0),
            Up = new Vector3D<float>(0, 1, 0),
            MovementSpeed = 5f,
            MouseSensitivity = 0.1f,
            ZoomSpeed = 1f,
            IsStatic = false,
            WorldUp = Vector3D<float>.UnitY
        };
    }

    public static Camera CreateOrthographic(float size = 10f, float aspectRatio = 16f / 9f)
    {
        return new Camera
        {
            ProjectionType = ProjectionType.Orthographic,
            OrthographicSize = size,
            AspectRatio = aspectRatio,
            NearPlane = -10f,
            FarPlane = 100f,
            Forward = new Vector3D<float>(0, 0, -1),
            Right = new Vector3D<float>(1, 0, 0),
            Up = new Vector3D<float>(0, 1, 0),
            IsStatic = true,
            StaticPosition = Vector3D<float>.Zero,
            WorldUp = Vector3D<float>.UnitY
        };
    }

    public static Camera CreateOrbit(
        Vector3D<float> target,
        float distance = 5f,
        float yaw = 0f,
        float pitch = 30f,
        float fov = 60f,
        float aspectRatio = 16f / 9f)
    {
        return new Camera
        {
            ProjectionType = ProjectionType.Perspective,
            FieldOfView = fov,
            AspectRatio = aspectRatio,
            NearPlane = 0.1f,
            FarPlane = 100f,
            Forward = new Vector3D<float>(0, 0, -1),
            Right = new Vector3D<float>(1, 0, 0),
            Up = new Vector3D<float>(0, 1, 0),
            WorldUp = Vector3D<float>.UnitY,
            IsStatic = true,
            IsOrbitMode = true,
            OrbitTarget = target,
            OrbitDistance = distance,
            OrbitYaw = yaw,
            OrbitPitch = pitch,
            OrbitMinDistance = 2f,
            OrbitMaxDistance = 10f,
            OrbitRotationSpeed = 0.3f,
            OrbitZoomSpeed = 0.5f
        };
    }
}
