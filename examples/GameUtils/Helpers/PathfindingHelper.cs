using Silk.NET.Maths;

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
    public static List<Vector2D<float>>? FindPath( Vector2D<float> start, Vector2D<float> end, int gridWidth, int gridHeight,
        Func<int, int, bool> isWalkable )
    {
        Queue<Vector2D<float>> queue = new();
        Dictionary<Vector2D<float>, Vector2D<float>?> cameFrom = new();

        queue.Enqueue( start );
        cameFrom[ start ] = null;

        Vector2D<float>[] directions = [ new( 1, 0 ), new( -1, 0 ), new( 0, 1 ), new( 0, -1 ) ];

        while ( queue.Count > 0 )
        {
            Vector2D<float> current = queue.Dequeue();

            if ( current.X == end.X && current.Y == end.Y )
            {
                return ReconstructPath( cameFrom, start, end );
            }

            foreach ( Vector2D<float> dir in directions )
            {
                Vector2D<float> next = current + dir;
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

    private static List<Vector2D<float>> ReconstructPath( Dictionary<Vector2D<float>, Vector2D<float>?> cameFrom, Vector2D<float> start, Vector2D<float> end )
    {
        List<Vector2D<float>> path = [ ];
        Vector2D<float>? current = end;

        while ( current != null && !(current.Value.X == start.X && current.Value.Y == start.Y) )
        {
            path.Add( current.Value );
            current = cameFrom[ current.Value ];
        }

        path.Reverse();
        return path;
    }
}
