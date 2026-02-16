using System.Numerics;
using System.Runtime.InteropServices;

namespace Aether.Core.Structures;

public struct QuadVertex( Vector3 position, Vector2 uv, Vector4 color, float texIndex = 0 )
{
    public Vector3 Position = position;
    public Vector2 Uv = uv;
    public Vector4 Color = color;
    public float TexIndex = texIndex;

    public static int SizeInBytes => Marshal.SizeOf<QuadVertex>();
}