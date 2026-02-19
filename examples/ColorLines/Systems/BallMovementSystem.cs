using Aether.Core;
using ColorLines.Components;
using Silk.NET.Maths;
using GameUtils.Helpers;

namespace ColorLines.Systems;

/// <summary>
/// Система перемещения шаров
/// </summary>
public class BallMovementSystem : SystemBase
{
    protected override void OnUpdate(float deltaTime)
    {
        foreach (Entity gameEntity in World.Filter<BoardComponent>().With<SelectedCellComponent>())
        {
            if (!World.Has<TargetCellComponent>(gameEntity))
                continue;
                
            // Если уже есть путь, пропускаем
            if (World.Has<MovementPathComponent>(gameEntity))
                continue;

            ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
            ref SelectedCellComponent selected = ref World.Get<SelectedCellComponent>(gameEntity);
            ref TargetCellComponent target = ref World.Get<TargetCellComponent>(gameEntity);

            // Копируем данные для использования в лямбде
            int[,] boardCells = board.Cells;
            int targetX = target.Position.X;
            int targetY = target.Position.Y;

            // Ищем путь
            Vector2D<float> from = new(selected.Position.X, selected.Position.Y);
            Vector2D<float> to = new(target.Position.X, target.Position.Y);

            List<Vector2D<float>>? pathFloat = PathfindingHelper.FindPath(from, to, 
                GameConstants.BoardSize, GameConstants.BoardSize,
                (x, y) => boardCells[x, y] == 0 || (x == targetX && y == targetY));

            if (pathFloat != null && pathFloat.Count > 0)
            {
                // Конвертируем путь в int
                List<Vector2D<int>> path = pathFloat.Select(p => 
                    new Vector2D<int>((int)p.X, (int)p.Y)).ToList();

                // Добавляем компоненты для анимации
                World.Add(gameEntity, new MovementPathComponent
                {
                    Path = path,
                    CurrentIndex = 0
                });

                World.Add(gameEntity, new MovementTimerComponent
                {
                    Time = 0f
                });
            }
            else
            {
                // Путь не найден - убираем целевую ячейку
                World.Remove<TargetCellComponent>(gameEntity);
            }
        }
    }
}
