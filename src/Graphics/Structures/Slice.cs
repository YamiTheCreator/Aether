using Silk.NET.Maths;

namespace Graphics.Structures;

public struct Slice( Vector2D<float> posMin, Vector2D<float> posMax, Vector2D<float> uvMin, Vector2D<float> uvMax )
{
    public Vector2D<float> PositionMin = posMin;
    public Vector2D<float> PositionMax = posMax;
    public Vector2D<float> UvMin = uvMin;
    public Vector2D<float> UvMax = uvMax;
}