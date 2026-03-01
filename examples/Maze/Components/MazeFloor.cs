using Aether.Core;
using Silk.NET.Maths;

namespace Maze.Components;

public struct MazeFloor : Component
{
    public Vector2D<float> Size;
    
    public bool IsGenerated;
}
