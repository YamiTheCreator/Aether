using Aether.Core;
using Graphics.Components;
using Silk.NET.Maths;

namespace Asteroids.Components;

public struct Particle : Component
{
    public Vector2D<float> Velocity;
    public float Lifetime;
    public float MaxLifetime;
    public Vector4D<float> Color;
}