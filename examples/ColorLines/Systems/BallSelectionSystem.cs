using Aether.Core;
using ColorLines.Components;
using Graphics.Components;
using Silk.NET.Maths;
using Silk.NET.Input;
using GameUtils.Helpers;

namespace ColorLines.Systems;

/// <summary>
/// Система выбора и перемещения шаров
/// </summary>
public class BallSelectionSystem : SystemBase
{
    private bool _previousMouseState;

    protected override void OnUpdate(float deltaTime)
    {
        // Пропускаем если игра окончена
        foreach (Entity e in World.Filter<GameOverComponent>())
        {
            return;
        }

        // Пропускаем если идет анимация
        foreach (Entity e in World.Filter<MovementPathComponent>())
        {
            return;
        }

        // Проверяем клик мыши
        if (!InputHelper.IsMouseJustPressed(MouseButton.Left, ref _previousMouseState))
            return;

        Vector2D<int>? clickedCell = GetCellFromMouse();
        if (clickedCell == null)
            return;

        // Находим игровую сущность
        foreach (Entity gameEntity in World.Filter<BoardComponent>())
        {
            ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
            int x = clickedCell.Value.X;
            int y = clickedCell.Value.Y;

            // Если нет выбранной ячейки
            if (!World.Has<SelectedCellComponent>(gameEntity))
            {
                // Выбираем ячейку если там есть шар
                if (board.Cells[x, y] != 0)
                {
                    World.Add(gameEntity, new SelectedCellComponent
                    {
                        Position = clickedCell.Value
                    });
                }
            }
            else
            {
                ref SelectedCellComponent selected = ref World.Get<SelectedCellComponent>(gameEntity);

                // Если кликнули на ту же ячейку - снимаем выбор
                if (selected.Position.X == x && selected.Position.Y == y)
                {
                    World.Remove<SelectedCellComponent>(gameEntity);
                }
                // Если кликнули на другой шар - переключаем выбор
                else if (board.Cells[x, y] != 0)
                {
                    selected.Position = clickedCell.Value;
                }
                // Если кликнули на пустую ячейку - пытаемся переместить
                else
                {
                    World.Add(gameEntity, new TargetCellComponent
                    {
                        Position = clickedCell.Value
                    });
                }
            }

            break;
        }
    }

    private Vector2D<int>? GetCellFromMouse()
    {
        Vector2D<float> mousePos = InputHelper.GetMousePosition();

        Camera camera = default;
        foreach (Entity e in World.Filter<Camera>())
        {
            camera = World.Get<Camera>(e);
            break;
        }

        Vector2D<float> worldPos = InputHelper.ScreenToWorld(mousePos, camera);

        const float boardStartX = -9f;
        const float boardStartY = -9f;
        const float cellSize = 1.8f;

        Vector2D<float>? cellFloat = InputHelper.WorldToCell(worldPos, 
            new Vector2D<float>(boardStartX, boardStartY), cellSize,
            GameConstants.BoardSize, GameConstants.BoardSize);

        if (cellFloat == null) return null;

        return new Vector2D<int>((int)cellFloat.Value.X, (int)cellFloat.Value.Y);
    }
}
