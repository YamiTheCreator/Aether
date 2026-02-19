using Silk.NET.Maths;
using Aether.Core;

namespace Tetris.Components;

public struct TetrisBoardComponent : Component
{
    public int[,] Board;
}

public struct TetrisPieceComponent : Component
{
    public TetrominoType Type;
    public int Rotation;
    public Vector2D<float> Position;
}

public struct TetrisGameStateComponent : Component
{
    public int Score;
    public int Level;
    public int LinesCleared;
    public bool IsGameOver;
    public TetrominoType NextType;
}

public struct TetrisTimerComponent : Component
{
    public float DropTimer;
    public float DropInterval;
    public float LockTimer;
    public bool IsLocking;
    public float MoveTimer;
    public float RotateTimer;
}

public enum TetrominoType
{
    None,
    I, // Cyan
    O, // Yellow
    T, // Purple
    S, // Green
    Z, // Red
    J, // Blue
    L // Orange
}
