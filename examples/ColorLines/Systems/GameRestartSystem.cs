using Aether.Core;
using ColorLines.Components;
using Silk.NET.Input;
using Silk.NET.Maths;
using GameUtils.Helpers;
using Graphics.Components;
using Graphics.Systems;

namespace ColorLines.Systems;

/// <summary>
/// Система перезапуска игры
/// </summary>
public class GameRestartSystem : SystemBase
{
    private readonly Random _random = new();

    protected override void OnUpdate(float deltaTime)
    {
        // Проверяем нажатие R только если игра окончена
        foreach (Entity gameEntity in World.Filter<GameOverComponent>())
        {
            InputSystem inputSystem = World.GetGlobal<Graphics.Systems.InputSystem>();
            Input input = World.GetGlobal<Graphics.Components.Input>();

            if (inputSystem.IsKeyPressed(input, Key.R))
            {
                RestartGame(gameEntity);
            }

            break;
        }
    }

    private void RestartGame(Entity gameEntity)
    {
        // Очищаем поле
        ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
        for (int x = 0; x < GameConstants.BoardSize; x++)
        {
            for (int y = 0; y < GameConstants.BoardSize; y++)
            {
                board.Cells[x, y] = 0;
            }
        }

        // Сбрасываем счет
        if (World.Has<ScoreComponent>(gameEntity))
        {
            ref ScoreComponent score = ref World.Get<ScoreComponent>(gameEntity);
            score.Value = 0;
        }

        // Генерируем новые следующие шары
        ref NextBallsComponent nextBalls = ref World.Get<NextBallsComponent>(gameEntity);
        for (int i = 0; i < GameConstants.BallsPerTurn; i++)
        {
            nextBalls.Colors[i] = _random.Next(1, GameConstants.ColorsCount + 1);
        }

        // Спавним начальные шары
        for (int i = 0; i < 5; i++)
        {
            List<Vector2D<float>> emptyCells = GridHelper.GetEmptyCells(board.Cells);
            if (emptyCells.Count == 0) break;

            Vector2D<float> cell = emptyCells[_random.Next(emptyCells.Count)];
            int color = _random.Next(1, GameConstants.ColorsCount + 1);
            board.Cells[(int)cell.X, (int)cell.Y] = color;
        }

        // Убираем все компоненты состояния
        World.Remove<GameOverComponent>(gameEntity);
        if (World.Has<SelectedCellComponent>(gameEntity))
            World.Remove<SelectedCellComponent>(gameEntity);
        if (World.Has<TargetCellComponent>(gameEntity))
            World.Remove<TargetCellComponent>(gameEntity);
        if (World.Has<MovementPathComponent>(gameEntity))
            World.Remove<MovementPathComponent>(gameEntity);
        if (World.Has<MovementTimerComponent>(gameEntity))
            World.Remove<MovementTimerComponent>(gameEntity);
    }
}
