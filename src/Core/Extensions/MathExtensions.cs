using Silk.NET.Maths;

namespace Aether.Core.Extensions;

public static class MathExtensions
{
    public static float DegToRad( float degrees ) =>
        degrees * ( MathF.PI / 180 );

    public static float Lerp( float a, float b, float t ) =>
        a + ( b - a ) * t;

    public static Vector2D<float> Lerp( Vector2D<float> a, Vector2D<float> b, float t ) =>
        new( Lerp( a.X, b.X, t ), Lerp( a.Y, b.Y, t ) );

    public static Vector3D<float> Lerp( Vector3D<float> a, Vector3D<float> b, float t ) =>
        new( Lerp( a.X, b.X, t ), Lerp( a.Y, b.Y, t ), Lerp( a.Z, b.Z, t ) );

    public static Vector4D<float> Lerp( Vector4D<float> a, Vector4D<float> b, float t ) =>
        new( Lerp( a.X, b.X, t ), Lerp( a.Y, b.Y, t ), Lerp( a.Z, b.Z, t ), Lerp( a.W, b.W, t ) );

    /// <summary>
    /// Converts hex color string to Vector3D (RGB 0-1 range)
    /// </summary>
    /// <param name="hex">Hex color string (e.g., "ff0000" or "#ff0000")</param>
    public static Vector3D<float> HexToColor( string hex )
    {
        // Remove # if present
        if ( hex.StartsWith( "#" ) )
            hex = hex.Substring( 1 );

        int r = Convert.ToInt32( hex.Substring( 0, 2 ), 16 );
        int g = Convert.ToInt32( hex.Substring( 2, 2 ), 16 );
        int b = Convert.ToInt32( hex.Substring( 4, 2 ), 16 );
        
        return new Vector3D<float>( r / 255f, g / 255f, b / 255f );
    }

    /// <summary>
    /// Converts hex color string to Vector4D (RGBA 0-1 range)
    /// </summary>
    /// <param name="hex">Hex color string (e.g., "ff0000ff" or "#ff0000ff")</param>
    public static Vector4D<float> HexToColorAlpha( string hex )
    {
        // Remove # if present
        if ( hex.StartsWith( "#" ) )
            hex = hex.Substring( 1 );

        int r = Convert.ToInt32( hex.Substring( 0, 2 ), 16 );
        int g = Convert.ToInt32( hex.Substring( 2, 2 ), 16 );
        int b = Convert.ToInt32( hex.Substring( 4, 2 ), 16 );
        int a = hex.Length >= 8 ? Convert.ToInt32( hex.Substring( 6, 2 ), 16 ) : 255;
        
        return new Vector4D<float>( r / 255f, g / 255f, b / 255f, a / 255f );
    }
}