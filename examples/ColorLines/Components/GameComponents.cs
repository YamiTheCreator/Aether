using Silk.NET.Maths;
using Aether.Core;

namespace ColorLines.Components;

// Константы игры
public static class GameConstants
{
    public const int BoardSize = 9;
    public const int ColorsCount = 6;
    public const int BallsPerTurn = 3;
    public const int MinLineLength = 5;
    public const float MoveSpeed = 0.15f; // seconds per cell
}

// Игровое поле
public struct BoardComponent : Component
{
    public int[,] Cells; // 0 = empty, 1-6 = ball colors
}

// Счет игрока
public struct ScoreComponent : Component
{
    public int Value;
}

// Следующие шары для появления
public struct NextBallsComponent : Component
{
    public int[] Colors; // Массив из 3 цветов
}

// Выбранная ячейка
public struct SelectedCellComponent : Component
{
    public Vector2D<int> Position;
}

// Целевая ячейка для перемещения
public struct TargetCellComponent : Component
{
    public Vector2D<int> Position;
}

// Путь перемещения шара
public struct MovementPathComponent : Component
{
    public List<Vector2D<int>> Path;
    public int CurrentIndex;
}

// Таймер анимации перемещения
public struct MovementTimerComponent : Component
{
    public float Time;
}

// Маркер окончания игры
public struct GameOverComponent : Component
{
}
