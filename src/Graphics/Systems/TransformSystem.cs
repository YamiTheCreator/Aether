using Aether.Core.Extensions;
using Graphics.Components;
using Silk.NET.Maths;

namespace Graphics.Systems;

public class TransformSystem
{
    public static Matrix4X4<float> GetWorldMatrix( ref Transform transform )
    {
        if ( !transform.IsDirty )
            return transform.CachedMatrix;

        transform.CachedMatrix =
            Matrix4X4.CreateScale( transform.Scale ) *
            Matrix4X4.CreateFromQuaternion( transform.Rotation ) *
            Matrix4X4.CreateTranslation( transform.Position );

        transform.IsDirty = false;
        return transform.CachedMatrix;
    }

    public static Matrix4X4<float> CreateModelMatrix( Vector3D<float> position, Quaternion<float> rotation,
        Vector3D<float> scale )
    {
        return Matrix4X4.CreateScale( scale ) *
               Matrix4X4.CreateFromQuaternion( rotation ) *
               Matrix4X4.CreateTranslation( position );
    }
}