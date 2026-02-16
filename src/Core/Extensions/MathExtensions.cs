namespace Aether.Core.Extensions;

public static class MathExtensions
{
    public static float RadToDeg( float degrees ) =>
        degrees * ( 180 / MathF.PI );

    public static float DegToRad( float degrees ) =>
        degrees * ( MathF.PI / 180 );
}