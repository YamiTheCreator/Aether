using Aether.Core;

namespace Maze.Components;

public struct Grid : Component
{
    public int[,] Layout;
    public int Width;
    public int Height;
}
