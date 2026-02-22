using Silk.NET.Maths;

namespace Aether.Core.Extensions;

public static class MathExtensions
{
    public static float DegToRad( float degrees ) =>
        degrees * ( MathF.PI / 180 );
}