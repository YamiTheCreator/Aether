using Aether.Core;
using Silk.NET.Maths;

namespace Asteroids.Components;

public struct Asteroid : Component
{
    public Vector2D<float> Velocity;
    public Vector2D<float>[] LocalVertices;
    public int Seed;
}