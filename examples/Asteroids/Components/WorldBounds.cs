using Aether.Core;

namespace Asteroids.Components;

public struct WorldBounds : Component
{
    public float Width;
    public float Height;

    public static WorldBounds Default => new()
    {
        Width = 20f,
        Height = 15f
    };
}
