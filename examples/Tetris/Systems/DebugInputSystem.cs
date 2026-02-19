using Aether.Core;
using Silk.NET.Input;

namespace Tetris.Systems;

/// <summary>
/// Debug system to test keyboard input
/// </summary>
public class DebugInputSystem : SystemBase
{
    protected override void OnUpdate(float deltaTime)
    {
        var inputSystem = World.GetGlobal<Graphics.Systems.InputSystem>();
        var input = World.GetGlobal<Graphics.Components.Input>();

        if (inputSystem.IsKeyPressed(input, Key.Left))
        {
            Console.WriteLine("Left key pressed!");
        }

        if (inputSystem.IsKeyPressed(input, Key.Right))
        {
            Console.WriteLine("Right key pressed!");
        }

        if (inputSystem.IsKeyPressed(input, Key.Up))
        {
            Console.WriteLine("Up key pressed!");
        }

        if (inputSystem.IsKeyPressed(input, Key.Down))
        {
            Console.WriteLine("Down key pressed!");
        }

        if (inputSystem.IsKeyPressed(input, Key.Space))
        {
            Console.WriteLine("Space key pressed!");
        }

        if (inputSystem.IsKeyPressed(input, Key.R))
        {
            Console.WriteLine("R key pressed!");
        }
    }
}
