using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Graphics.Structures;

public struct Vertex( Vector3D<float> position, Vector2D<float> uv, Vector4D<float> color, float texIndex = 0 )
{
    public Vector3D<float> Position = position;
    public Vector2D<float> Uv = uv;
    public Vector4D<float> Color = color;
    public float TexIndex = texIndex;

    public static int SizeInBytes => Marshal.SizeOf<Vertex>();
}