using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using Silk.NET.Input;
using Tetris.Components;

namespace Tetris.Systems;

/// <summary>
/// System responsible for handling player input
/// </summary>
public class TetrisInputSystem : SystemBase
{
    private TetrisLogicSystem? _logicSystem;
    private const float MoveDelay = 0.15f;
    private const float RotateDelay = 0.2f;

    protected override void OnInit()
    {
        // Get reference to logic system
        _logicSystem = null;
        foreach (SystemBase system in World.GetType().GetField("_updateSystems",
                     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 ?.GetValue(World) as List<SystemBase> ?? [])
        {
            if (system is TetrisLogicSystem logicSystem)
            {
                _logicSystem = logicSystem;
                break;
            }
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (_logicSystem == null)
            return;

        InputSystem inputSystem = World.GetGlobal<InputSystem>();
        Input input = World.GetGlobal<Input>();

        foreach (Entity entity in World.Filter<TetrisGameStateComponent>().With<TetrisTimerComponent>())
        {
            if (!World.Has<TetrisPieceComponent>(entity) || !World.Has<TetrisBoardComponent>(entity))
                continue;
            
            ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>(entity);
            ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>(entity);

            if (state.IsGameOver)
            {
                if (inputSystem.IsKeyPressed(input, Key.R))
                {
                    _logicSystem.ResetGame();
                }
                return;
            }

            // Left movement
            if (inputSystem.IsKeyPressed(input, Key.Left) || 
                (inputSystem.IsKeyDown(input, Key.Left) && timers.MoveTimer <= 0))
            {
                _logicSystem.TryMovePiece(entity, -1, 0);
                timers.MoveTimer = MoveDelay;
            }

            // Right movement
            if (inputSystem.IsKeyPressed(input, Key.Right) || 
                (inputSystem.IsKeyDown(input, Key.Right) && timers.MoveTimer <= 0))
            {
                _logicSystem.TryMovePiece(entity, 1, 0);
                timers.MoveTimer = MoveDelay;
            }

            // Soft drop (down key)
            if (inputSystem.IsKeyDown(input, Key.Down))
            {
                timers.DropTimer += deltaTime * 10f;
            }

            // Rotation
            if ((inputSystem.IsKeyPressed(input, Key.Up) || inputSystem.IsKeyPressed(input, Key.Z)) && 
                timers.RotateTimer <= 0)
            {
                _logicSystem.TryRotatePiece(entity);
                timers.RotateTimer = RotateDelay;
            }

            // Hard drop
            if (inputSystem.IsKeyPressed(input, Key.Space))
            {
                _logicSystem.HardDrop(entity);
            }
        }
    }
}
