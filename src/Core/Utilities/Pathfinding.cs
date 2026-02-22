using Silk.NET.Maths;

namespace Aether.Core.Utilities;

/// <summary>
/// BFS pathfinding for grid-based games
/// </summary>
public static class Pathfinding
{
    /// <summary>
    /// Finds shortest path from start to end using BFS
    /// </summary>
    /// <param name="start">Start cell</param>
    /// <param name="end">End cell</param>
    /// <param name="gridWidth">Grid width</param>
    /// <param name="gridHeight">Grid height</param>
    /// <param name="isWalkable">Function to check if cell is walkable</param>
    /// <returns>List of cells in path, or null if no path found</returns>
    public static List<Vector2D<int>>? FindPath( Vector2D<int> start, Vector2D<int> end, int gridWidth, int gridHeight, Func<int, int, bool> isWalkable )
    {
        Queue<Vector2D<int>> queue = new();
        Dictionary<Vector2D<int>, Vector2D<int>?> cameFrom = new();

        queue.Enqueue( start );
        cameFrom[ start ] = null;

        Vector2D<int>[] directions = [ new( 1, 0 ), new( -1, 0 ), new( 0, 1 ), new( 0, -1 ) ];

        while ( queue.Count > 0 )
        {
            Vector2D<int> current = queue.Dequeue();

            if ( current.X == end.X && current.Y == end.Y )
            {
                return ReconstructPath( cameFrom, start, end );
            }

            foreach ( Vector2D<int> dir in directions )
            {
                Vector2D<int> next = current + dir;

                if ( !GridHelper.IsInBounds( next.X, next.Y, gridWidth, gridHeight ) )
                    continue;

                if ( !isWalkable( next.X, next.Y ) )
                    continue;

                if ( cameFrom.ContainsKey( next ) )
                    continue;

                queue.Enqueue( next );
                cameFrom[ next ] = current;
            }
        }

        return null;
    }

    private static List<Vector2D<int>> ReconstructPath( Dictionary<Vector2D<int>, Vector2D<int>?> cameFrom, Vector2D<int> start, Vector2D<int> end )
    {
        List<Vector2D<int>> path = [ ];
        Vector2D<int>? current = end;

        while ( current != null && !(current.Value.X == start.X && current.Value.Y == start.Y) )
        {
            path.Add( current.Value );
            current = cameFrom[ current.Value ];
        }

        path.Reverse();
        return path;
    }
}
