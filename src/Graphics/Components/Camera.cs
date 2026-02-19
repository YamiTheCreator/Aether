using Aether.Core;
using Aether.Core.Enums;
using Silk.NET.Maths;

namespace Graphics.Components;

public struct Camera() : Component
{
    public ProjectionType ProjectionType { get; set; } = ProjectionType.Perspective;

    // Perspective settings
    public float FieldOfView { get; set; } = 45.0f;
    public float AspectRatio { get; set; } = 16.0f / 9.0f;

    // Orthographic settings
    public float OrthographicSize { get; set; } = 10.0f;

    // Common settings
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 100.0f;

    // Camera orientation (for mouse control)
    public float Yaw { get; set; } = -90.0f;
    public float Pitch { get; set; } = 0.0f;

    // Camera control settings
    public float MovementSpeed { get; set; } = 5.0f;
    public float MouseSensitivity { get; set; } = 0.1f;
    public float ZoomSpeed { get; set; } = 1.0f;

    public bool IsStatic { get; set; } = false;

    public Vector3D<float> StaticPosition { get; set; } = Vector3D<float>.Zero;
    public Vector3D<float> WorldUp { get; set; } = Vector3D<float>.UnitY;

    // Cached matrices
    public Matrix4X4<float> ViewMatrix { get; set; } = Matrix4X4<float>.Identity;
    public Matrix4X4<float> ProjectionMatrix { get; set; } = Matrix4X4<float>.Identity;
    public Matrix4X4<float> ViewProjectionMatrix { get; set; } = Matrix4X4<float>.Identity;
}