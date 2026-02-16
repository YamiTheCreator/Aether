using System.Numerics;

namespace Aether.Core.Structures;

public struct Glyph( Vector2 size, Vector2 bearing, float advance, Vector2 uvMin, Vector2 uvMax )
{
    public Vector2 Size = size;
    public Vector2 Bearing = bearing;
    public float Advance = advance;
    public Vector2 UvMin = uvMin;
    public Vector2 UvMax = uvMax;
}