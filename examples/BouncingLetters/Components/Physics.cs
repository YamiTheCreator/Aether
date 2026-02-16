using Aether.Core;
using System.Numerics;

namespace BouncingLetters.Components;

public struct Physics( float bounciness = 1.0f ) : IComponent
{
    public Vector3 Velocity = Vector3.Zero;
    public readonly float Bounciness = bounciness;
}