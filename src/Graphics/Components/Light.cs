using Aether.Core;
using Silk.NET.Maths;

namespace Graphics.Components;

public struct Light : Component
{
    public bool Enabled;
    public Vector3D<float> AmbientColor;
    public Vector3D<float> DiffuseColor;
    public Vector3D<float> SpecularColor;
    public float Intensity;
}