using Aether.Core;
using Aether.Core.Enums;
using Silk.NET.Maths;

namespace Graphics.Components;

public struct Camera : Component
{
    public ProjectionType ProjectionType;

    public float FieldOfView;
    public float AspectRatio;

    public float OrthographicSize;

    public float NearPlane;
    public float FarPlane;

    public Matrix4X4<float> ViewMatrix;
    public Matrix4X4<float> ProjectionMatrix;
    public Matrix4X4<float> ViewProjectionMatrix;
}