using Silk.NET.Maths;

namespace Aether.Core.Extensions;

public static class MathExtensions
{
    public static float DegToRad( float degrees ) =>
        degrees * ( MathF.PI / 180 );
    
    public static float GetAngleFromQuaternion( Quaternion<float> rotation )
    {
        return 2f * MathF.Atan2( rotation.Z, rotation.W );
    }
}