using Aether.Core;
using Silk.NET.Maths;

namespace Asteroids.Components;

public struct Bullet : Component
{
    public Vector2D<float> Velocity;
    public float Lifetime;
}
