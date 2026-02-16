using System.Numerics;

namespace GameUtils.Helpers;

/// <summary>
/// Predefined color palettes for games
/// </summary>
public static class ColorPalette
{
    /// <summary>
    /// Tetris colors (7 pieces)
    /// </summary>
    public static readonly Vector4[] Tetris =
    [
        new( 0.0f, 1.0f, 1.0f, 1.0f ), // Cyan (I)
        new( 1.0f, 1.0f, 0.0f, 1.0f ), // Yellow (O)
        new( 0.5f, 0.0f, 0.5f, 1.0f ), // Purple (T)
        new( 0.0f, 1.0f, 0.0f, 1.0f ), // Green (S)
        new( 1.0f, 0.0f, 0.0f, 1.0f ), // Red (Z)
        new( 0.0f, 0.0f, 1.0f, 1.0f ), // Blue (J)
        new( 1.0f, 0.5f, 0.0f, 1.0f ) // Orange (L)
    ];

    /// <summary>
    /// ColorLines colors (6 ball colors)
    /// </summary>
    public static readonly Vector4[] ColorLines =
    [
        new( 1.0f, 0.0f, 0.0f, 1.0f ), // Red
        new( 0.0f, 1.0f, 0.0f, 1.0f ), // Green
        new( 0.0f, 0.0f, 1.0f, 1.0f ), // Blue
        new( 1.0f, 1.0f, 0.0f, 1.0f ), // Yellow
        new( 1.0f, 0.0f, 1.0f, 1.0f ), // Magenta
        new( 0.0f, 1.0f, 1.0f, 1.0f ) // Cyan
    ];

    /// <summary>
    /// Rainbow colors (general purpose)
    /// </summary>
    public static readonly Vector4[] Rainbow =
    [
        new( 1.0f, 0.0f, 0.0f, 1.0f ), // Red
        new( 1.0f, 0.5f, 0.0f, 1.0f ), // Orange
        new( 1.0f, 1.0f, 0.0f, 1.0f ), // Yellow
        new( 0.0f, 1.0f, 0.0f, 1.0f ), // Green
        new( 0.0f, 0.0f, 1.0f, 1.0f ), // Blue
        new( 0.3f, 0.0f, 0.5f, 1.0f ), // Indigo
        new( 0.5f, 0.0f, 0.5f, 1.0f ) // Violet
    ];

    /// <summary>
    /// Gets color from palette by index (wraps around)
    /// </summary>
    public static Vector4 GetColor( Vector4[] palette, int index )
    {
        return palette[ index % palette.Length ];
    }
}
