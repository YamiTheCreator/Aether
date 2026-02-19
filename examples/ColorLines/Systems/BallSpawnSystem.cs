using Aether.Core;
using ColorLines.Components;
using Silk.NET.Maths;
using GameUtils.Helpers;

namespace ColorLines.Systems;

/// <summary>
/// Система спавна новых шаров
/// </summary>
public class BallSpawnSystem : SystemBase
{
    private readonly Random _random = new();

    protected override void OnUpdate(float deltaTime)
    {
        foreach (Entity gameEntity in World.Filter<BoardComponent>().With<SpawnBallsComponent>())
        {
            ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
            ref NextBallsComponent nextBalls = ref World.Get<NextBallsComponent>(gameEntity);

            SpawnNextBalls(gameEntity, ref board, ref nextBalls);

            // Генерируем новые следующие шары
            GenerateNextBalls(ref nextBalls);

            // Убираем маркер спавна
            World.Remove<SpawnBallsComponent>(gameEntity);

            // Проверяем game over
            CheckGameOver(gameEntity, ref board);
        }
    }

    private void SpawnNextBalls(Entity gameEntity, ref BoardComponent board, ref NextBallsComponent nextBalls)
    {
        List<Vector2D<float>> emptyCells = GridHelper.GetEmptyCells(board.Cells);

        for (int i = 0; i < GameConstants.BallsPerTurn && emptyCells.Count > 0; i++)
        {
            int index = _random.Next(emptyCells.Count);
            Vector2D<float> cell = emptyCells[index];
            emptyCells.RemoveAt(index);

            int x = (int)cell.X;
            int y = (int)cell.Y;
            board.Cells[x, y] = nextBalls.Colors[i];

            // Копируем для лямбды
            int[,] boardCells = board.Cells;
            
            // Проверяем линии после спавна
            HashSet<Vector2D<float>> toRemove = LineDetectionHelper.FindMatchingLines(
                x, y, GameConstants.BoardSize, GameConstants.BoardSize,
                (cx, cy) => boardCells[cx, cy], GameConstants.MinLineLength);

            if (toRemove.Count > 0)
            {
                // Вычисляем очки
                int ballsRemoved = toRemove.Count;
                int points = ballsRemoved switch
                {
                    5 => 10,
                    6 => 15,
                    7 => 20,
                    8 => 30,
                    >= 9 => 50,
                    _ => 0
                };

                // Добавляем очки
                if (World.Has<ScoreComponent>(gameEntity))
                {
                    ref ScoreComponent score = ref World.Get<ScoreComponent>(gameEntity);
                    score.Value += points;
                }

                // Удаляем шары
                foreach (Vector2D<float> removeCell in toRemove)
                {
                    board.Cells[(int)removeCell.X, (int)removeCell.Y] = 0;
                }
            }
        }
    }

    private void GenerateNextBalls(ref NextBallsComponent nextBalls)
    {
        for (int i = 0; i < GameConstants.BallsPerTurn; i++)
        {
            nextBalls.Colors[i] = _random.Next(1, GameConstants.ColorsCount + 1);
        }
    }

    private void CheckGameOver(Entity gameEntity, ref BoardComponent board)
    {
        int emptyCells = GridHelper.CountEmptyCells(board.Cells);
        if (emptyCells == 0)
        {
            World.Add(gameEntity, new GameOverComponent());
        }
    }
}
