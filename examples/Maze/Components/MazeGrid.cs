using Aether.Core;

namespace Maze.Components;

public struct MazeGrid : Component
{
    public int[,] Layout;
    public int Width;
    public int Height;
    public bool IsGenerated;
}
