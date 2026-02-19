using Silk.NET.Maths;

namespace Graphics.Components;

public struct Material
{
    public Shader Shader { get; set; }
    public Texture2D Texture { get; set; }
    public Vector4D<float> Color { get; set; }
}