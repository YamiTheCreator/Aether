using Aether.Core;
using Silk.NET.Maths;

namespace Galaxian3D.Components;

public struct Spaceship : Component
{
    public Vector2D<float> Velocity;
    public float AngularVelocity;
    public float ShootCooldown;
}