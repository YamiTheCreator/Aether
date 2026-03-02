using Aether.Core;

namespace Asteroids.Components;

public struct SpaceshipConfig : Component
{
    public float MaxSpeed;
    public float Acceleration;
    public float RotationSpeed;
    public float LinearDrag;
    public float AngularDrag;
    public float ShootCooldown;

    public static SpaceshipConfig Default => new()
    {
        MaxSpeed = 5f,
        Acceleration = 10f,
        RotationSpeed = 25f,
        LinearDrag = 0.98f,
        AngularDrag = 0.96f,
        ShootCooldown = 0.5f
    };
}
