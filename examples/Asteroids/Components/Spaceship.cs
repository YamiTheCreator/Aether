using Aether.Core;
using Silk.NET.Maths;

namespace Asteroids.Components;

public struct Spaceship : Component
{
    public Vector2D<float> Velocity;
    public float AngularVelocity;
    public float ShootCooldown;
    public Vector2D<float>[] LocalVertices;
}
