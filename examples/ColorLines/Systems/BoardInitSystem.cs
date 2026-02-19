using Aether.Core;
using ColorLines.Components;
using Silk.NET.Maths;
using GameUtils.Helpers;

namespace ColorLines.Systems;

/// <summary>
/// Система инициализации игрового поля
/// </summary>
public class BoardInitSystem : SystemBase
{
    private readonly Random _random = new();

    protected override void OnInit()
    {
        // Создаем игровую сущность
        Entity gameEntity = World.Spawn();

        // Добавляем компоненты
        World.Add(gameEntity, new BoardComponent
        {
            Cells = new int[GameConstants.BoardSize, GameConstants.BoardSize]
        });

        World.Add(gameEntity, new ScoreComponent { Value = 0 });

        World.Add(gameEntity, new NextBallsComponent
        {
            Colors = new int[GameConstants.BallsPerTurn]
        });

        // Генерируем первые следующие шары
        ref NextBallsComponent nextBalls = ref World.Get<NextBallsComponent>(gameEntity);
        GenerateNextBalls(ref nextBalls);

        // Спавним начальные шары
        ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
        SpawnInitialBalls(ref board);
    }

    private void GenerateNextBalls(ref NextBallsComponent nextBalls)
    {
        for (int i = 0; i < GameConstants.BallsPerTurn; i++)
        {
            nextBalls.Colors[i] = _random.Next(1, GameConstants.ColorsCount + 1);
        }
    }

    private void SpawnInitialBalls(ref BoardComponent board)
    {
        for (int i = 0; i < 5; i++)
        {
            List<Vector2D<float>> emptyCells = GridHelper.GetEmptyCells(board.Cells);
            if (emptyCells.Count == 0) break;

            Vector2D<float> cell = emptyCells[_random.Next(emptyCells.Count)];
            int color = _random.Next(1, GameConstants.ColorsCount + 1);
            board.Cells[(int)cell.X, (int)cell.Y] = color;
        }
    }
}
