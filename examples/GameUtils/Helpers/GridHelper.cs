using System.Numerics;

namespace GameUtils.Helpers;

/// <summary>
/// Helper methods for grid-based games
/// </summary>
public static class GridHelper
{
    /// <summary>
    /// Counts empty cells in a 2D grid
    /// </summary>
    public static int CountEmptyCells( int[,] grid, int emptyValue = 0 )
    {
        int count = 0;
        int width = grid.GetLength( 0 );
        int height = grid.GetLength( 1 );

        for ( int x = 0; x < width; x++ )
        {
            for ( int y = 0; y < height; y++ )
            {
                if ( grid[ x, y ] == emptyValue )
                    count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets all empty cell positions
    /// </summary>
    public static List<Vector2> GetEmptyCells( int[,] grid, int emptyValue = 0 )
    {
        List<Vector2> cells = [ ];
        int width = grid.GetLength( 0 );
        int height = grid.GetLength( 1 );

        for ( int x = 0; x < width; x++ )
        {
            for ( int y = 0; y < height; y++ )
            {
                if ( grid[ x, y ] == emptyValue )
                    cells.Add( new Vector2( x, y ) );
            }
        }

        return cells;
    }

    /// <summary>
    /// Clears full rows and drops blocks above (Tetris-style)
    /// </summary>
    /// <returns>Number of lines cleared</returns>
    public static int ClearFullRows( int[,] grid, int emptyValue = 0 )
    {
        int width = grid.GetLength( 0 );
        int height = grid.GetLength( 1 );
        int linesCleared = 0;

        for ( int y = 0; y < height; y++ )
        {
            bool isFull = true;

            for ( int x = 0; x < width; x++ )
            {
                if ( grid[ x, y ] == emptyValue )
                {
                    isFull = false;
                    break;
                }
            }

            if ( isFull )
            {
                linesCleared++;

                // Clear row
                for ( int x = 0; x < width; x++ )
                    grid[ x, y ] = emptyValue;

                // Drop rows above
                for ( int yy = y; yy < height - 1; yy++ )
                {
                    for ( int x = 0; x < width; x++ )
                        grid[ x, yy ] = grid[ x, yy + 1 ];
                }

                // Clear top row
                for ( int x = 0; x < width; x++ )
                    grid[ x, height - 1 ] = emptyValue;

                y--; // Check same row again
            }
        }

        return linesCleared;
    }

    /// <summary>
    /// Checks if position is within grid bounds
    /// </summary>
    public static bool IsInBounds( int x, int y, int width, int height )
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
