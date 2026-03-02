using Aether.Core;

namespace Asteroids.Components;

public struct BulletConfig : Component
{
    public float Speed;
    public float DefaultLifetime;

    public static BulletConfig Default => new()
    {
        Speed = 15f,
        DefaultLifetime = 2f
    };
}
