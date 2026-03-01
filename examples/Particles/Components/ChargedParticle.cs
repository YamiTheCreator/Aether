using Silk.NET.Maths;
using Aether.Core;

namespace Particles.Components;

public struct ChargedParticle : Component
{
    public Vector4D<float> Color;
    public float Charge; // Положительный или отрицательный заряд
    public float Mass;
    public Vector2D<float> Velocity;
    public float Radius;
}