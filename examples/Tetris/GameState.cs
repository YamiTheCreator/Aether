using System.Numerics;

namespace Tetris;

public class GameState
{
    public const int Width = 10;
    public const int Height = 20;

    public readonly int[ , ] Board = new int[ Width, Height ];

    public TetrominoType CurrentType = TetrominoType.None;
    public int CurrentRotation = 0;
    public Vector2 CurrentPosition = new( 4, 19 );

    public TetrominoType NextType = TetrominoType.None;

    public float DropTimer = 0f;
    public float DropInterval = 1.0f;
    public readonly float LockDelay = 0.5f;
    public float LockTimer = 0f;
    public bool IsLocking = false;

    public bool IsGameOver = false;
    public int Level = 1;
    public int LinesCleared = 0;
    public int Score = 0;

    public float MoveTimer = 0f;
    public const float MoveDelay = 0.15f;
    public float RotateTimer = 0f;
    public const float RotateDelay = 0.2f;

    public bool IsOccupied( int x, int y )
    {
        if ( x is < 0 or >= Width ) return true;
        if ( y < 0 ) return true;
        if ( y >= Height ) return false;
        return Board[ x, y ] != 0;
    }

    public void PlaceBlock( int x, int y, int colorIndex )
    {
        if ( x is >= 0 and < Width && y is >= 0 and < Height )
        {
            Board[ x, y ] = colorIndex;
        }
    }
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