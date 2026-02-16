using System.Numerics;

namespace GameUtils.Helpers;

/// <summary>
/// Helper for detecting lines in grid-based games (match-3, ColorLines, etc)
/// </summary>
public static class LineDetectionHelper
{
    /// <summary>
    /// Finds all matching cells in horizontal, vertical, and diagonal lines from a starting position
    /// </summary>
    /// <param name="x">Start X coordinate</param>
    /// <param name="y">Start Y coordinate</param>
    /// <param name="gridWidth">Grid width</param>
    /// <param name="gridHeight">Grid height</param>
    /// <param name="getValue">Function to get value at cell</param>
    /// <param name="minLength">Minimum line length to count</param>
    /// <returns>HashSet of all cells in matching lines</returns>
    public static HashSet<Vector2> FindMatchingLines( int x, int y, int gridWidth, int gridHeight,
        Func<int, int, int> getValue, int minLength = 5 )
    {
        int value = getValue( x, y );
        if ( value == 0 )
            return [ ];

        HashSet<Vector2> result = [ ];

        // Check all 4 directions
        CheckLine( x, y, 1, 0, gridWidth, gridHeight, getValue, value, minLength, result ); // Horizontal
        CheckLine( x, y, 0, 1, gridWidth, gridHeight, getValue, value, minLength, result ); // Vertical
        CheckLine( x, y, 1, 1, gridWidth, gridHeight, getValue, value, minLength, result ); // Diagonal \
        CheckLine( x, y, 1, -1, gridWidth, gridHeight, getValue, value, minLength, result ); // Diagonal /

        return result;
    }

    private static void CheckLine( int x, int y, int dx, int dy, int gridWidth, int gridHeight,
        Func<int, int, int> getValue, int targetValue, int minLength, HashSet<Vector2> result )
    {
        List<Vector2> line = [ new( x, y ) ];

        // Check negative direction
        for ( int i = 1;; i++ )
        {
            int nx = x - dx * i;
            int ny = y - dy * i;

            if ( nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight )
                break;

            if ( getValue( nx, ny ) != targetValue )
                break;

            line.Add( new Vector2( nx, ny ) );
        }

        // Check positive direction
        for ( int i = 1;; i++ )
        {
            int nx = x + dx * i;
            int ny = y + dy * i;

            if ( nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight )
                break;

            if ( getValue( nx, ny ) != targetValue )
                break;

            line.Add( new Vector2( nx, ny ) );
        }

        if ( line.Count >= minLength )
        {
            foreach ( Vector2 cell in line )
                result.Add( cell );
        }
    }
}
