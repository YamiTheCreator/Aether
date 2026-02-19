using System.Numerics;
using Aether.Core;
using Silk.NET.Maths;

namespace Graphics.Components;

public struct Transform( Vector3D<float> position, Quaternion<float> rotation, Vector3D<float> scale )
    : Component
{
    public Vector3D<float> Position = position;
    public Quaternion<float> Rotation = rotation;
    public Vector3D<float> Scale = scale;

    public bool IsDirty;

    public Matrix4X4<float> CachedMatrix;

    public Vector3D<float> Forward;
    public Vector3D<float> Right;
    public Vector3D<float> Up;

    public Transform( Vector3D<float> position ) : this( position, Quaternion<float>.Identity, Vector3D<float>.Zero ) { }
}