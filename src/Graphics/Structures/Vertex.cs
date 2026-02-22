using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace Graphics.Structures;

[StructLayout( LayoutKind.Sequential )]
public struct Vertex(
    Vector3D<float> position,
    Vector2D<float> uv,
    Vector4D<float> color,
    float texIndex = 0,
    Vector3D<float> normal = default,
    Vector3D<float> tangent = default,
    Vector3D<float> bitangent = default )
{
    public Vector3D<float> Position = position;
    public Vector2D<float> Uv = uv;
    public Vector4D<float> Color = color;
    public float TexIndex = texIndex;
    public Vector3D<float> Normal = normal == default ? new Vector3D<float>( 0, 0, 1 ) : normal;
    public Vector3D<float> Tangent = tangent == default ? new Vector3D<float>( 1, 0, 0 ) : tangent;
    public Vector3D<float> Bitangent = bitangent == default ? new Vector3D<float>( 0, 1, 0 ) : bitangent;

    public static int SizeInBytes => Marshal.SizeOf<Vertex>();
}