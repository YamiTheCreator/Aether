using Aether.Core;
using ColorLines.Components;

namespace ColorLines.Systems;

/// <summary>
/// Система анимации перемещения шаров
/// </summary>
public class BallAnimationSystem : SystemBase
{
    protected override void OnUpdate(float deltaTime)
    {
        foreach (Entity gameEntity in World.Filter<MovementPathComponent>().With<MovementTimerComponent>())
        {
            ref MovementPathComponent movement = ref World.Get<MovementPathComponent>(gameEntity);
            ref MovementTimerComponent timer = ref World.Get<MovementTimerComponent>(gameEntity);

            timer.Time += deltaTime;

            if (timer.Time >= GameConstants.MoveSpeed)
            {
                timer.Time = 0f;
                movement.CurrentIndex++;

                // Если достигли конца пути
                if (movement.CurrentIndex >= movement.Path.Count)
                {
                    // Завершаем перемещение
                    CompleteBallMove(gameEntity);
                }
            }
        }
    }

    private void CompleteBallMove(Entity gameEntity)
    {
        if (!World.Has<SelectedCellComponent>(gameEntity) || 
            !World.Has<TargetCellComponent>(gameEntity))
            return;

        ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
        ref SelectedCellComponent selected = ref World.Get<SelectedCellComponent>(gameEntity);
        ref TargetCellComponent target = ref World.Get<TargetCellComponent>(gameEntity);

        // Перемещаем шар
        int ballColor = board.Cells[selected.Position.X, selected.Position.Y];
        board.Cells[selected.Position.X, selected.Position.Y] = 0;
        board.Cells[target.Position.X, target.Position.Y] = ballColor;

        // Убираем компоненты перемещения
        World.Remove<MovementPathComponent>(gameEntity);
        World.Remove<MovementTimerComponent>(gameEntity);
        World.Remove<SelectedCellComponent>(gameEntity);
        World.Remove<TargetCellComponent>(gameEntity);

        // Добавляем маркер для проверки линий
        World.Add(gameEntity, new CheckLinesComponent
        {
            Position = target.Position
        });
    }
}

// Маркер для проверки линий после перемещения
public struct CheckLinesComponent : Component
{
    public Silk.NET.Maths.Vector2D<int> Position;
}
