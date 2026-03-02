using Aether.Core;
using Silk.NET.Maths;

namespace Asteroids.Components;

public struct Collider : Component
{
    public Vector2D<float>[] LocalVertices;
}
