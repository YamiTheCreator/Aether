using System.Numerics;
using Aether.Core;

namespace Graphics.Components;

public struct Transform( Vector3 position ) : IComponent
{
    public Vector3 Position = position;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4x4 WorldMatrix =>
        Matrix4x4.CreateScale( Scale ) *
        Matrix4x4.CreateFromQuaternion( Rotation ) *
        Matrix4x4.CreateTranslation( Position );

    public readonly Vector3 Forward => Vector3.Transform( -Vector3.UnitZ, Rotation );
    public readonly Vector3 Right => Vector3.Transform( Vector3.UnitX, Rotation );
    public readonly Vector3 Up => Vector3.Transform( Vector3.UnitY, Rotation );
}