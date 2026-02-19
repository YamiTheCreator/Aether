using Aether.Core;
using ColorLines.Components;
using Silk.NET.Maths;
using GameUtils.Helpers;

namespace ColorLines.Systems;

/// <summary>
/// Система проверки и удаления линий
/// </summary>
public class LineCheckSystem : SystemBase
{
    protected override void OnUpdate(float deltaTime)
    {
        foreach (Entity gameEntity in World.Filter<BoardComponent>().With<CheckLinesComponent>())
        {
            ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
            ref CheckLinesComponent check = ref World.Get<CheckLinesComponent>(gameEntity);

            bool linesRemoved = CheckAndRemoveLines(gameEntity, ref board, check.Position.X, check.Position.Y);

            // Убираем маркер проверки
            World.Remove<CheckLinesComponent>(gameEntity);

            // Если линии не были удалены, спавним новые шары
            if (!linesRemoved)
            {
                World.Add(gameEntity, new SpawnBallsComponent());
            }
        }
    }

    private bool CheckAndRemoveLines(Entity gameEntity, ref BoardComponent board, int x, int y)
    {
        int[,] boardCells = board.Cells;
        
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
            foreach (Vector2D<float> cell in toRemove)
            {
                board.Cells[(int)cell.X, (int)cell.Y] = 0;
            }

            return true;
        }

        return false;
    }
}

// Маркер для спавна новых шаров
public struct SpawnBallsComponent : Component
{
}
