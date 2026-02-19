using Silk.NET.Maths;

namespace Graphics.Structures;

public struct Glyph( Vector2D<float> size, Vector2D<float> bearing, float advance, Vector2D<float> uvMin, Vector2D<float> uvMax )
{
    public Vector2D<float> Size = size;
    public Vector2D<float> Bearing = bearing;
    public float Advance = advance;
    public Vector2D<float> UvMin = uvMin;
    public Vector2D<float> UvMax = uvMax;
}