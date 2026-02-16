using System.Numerics;

namespace GameUtils.Helpers;

/// <summary>
/// BFS pathfinding for grid-based games
/// </summary>
public static class PathfindingHelper
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
    public static List<Vector2>? FindPath( Vector2 start, Vector2 end, int gridWidth, int gridHeight,
        Func<int, int, bool> isWalkable )
    {
        Queue<Vector2> queue = new();
        Dictionary<Vector2, Vector2?> cameFrom = new();

        queue.Enqueue( start );
        cameFrom[ start ] = null;

        Vector2[] directions = [ new( 1, 0 ), new( -1, 0 ), new( 0, 1 ), new( 0, -1 ) ];

        while ( queue.Count > 0 )
        {
            Vector2 current = queue.Dequeue();

            if ( current == end )
            {
                return ReconstructPath( cameFrom, start, end );
            }

            foreach ( Vector2 dir in directions )
            {
                Vector2 next = current + dir;
                int nx = ( int )next.X;
                int ny = ( int )next.Y;

                if ( nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight )
                    continue;

                if ( !isWalkable( nx, ny ) )
                    continue;

                if ( cameFrom.ContainsKey( next ) )
                    continue;

                queue.Enqueue( next );
                cameFrom[ next ] = current;
            }
        }

        return null;
    }

    private static List<Vector2> ReconstructPath( Dictionary<Vector2, Vector2?> cameFrom, Vector2 start, Vector2 end )
    {
        List<Vector2> path = [ ];
        Vector2? current = end;

        while ( current != null && current != start )
        {
            path.Add( current.Value );
            current = cameFrom[ current.Value ];
        }

        path.Reverse();
        return path;
    }
}
