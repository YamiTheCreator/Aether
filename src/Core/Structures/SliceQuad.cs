using System.Numerics;

namespace Aether.Core.Structures;

public struct SliceQuad( Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax )
{
    public Vector2 PositionMin = posMin;
    public Vector2 PositionMax = posMax;
    public Vector2 UvMin = uvMin;
    public Vector2 UvMax = uvMax;
}