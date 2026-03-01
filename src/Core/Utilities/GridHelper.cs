using Silk.NET.Maths;

namespace Aether.Core.Utilities;

public static class GridHelper
{
    public static int ClearFullRows( int[ , ] grid, int emptyValue = 0 )
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

    public static bool IsInBounds( int x, int y, int width, int height )
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}