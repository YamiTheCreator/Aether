using System.Numerics;
using GameUtils.Helpers;

namespace ColorLines;

public class GameState
{
    public const int BoardSize = 9;
    public const int ColorsCount = 6;
    public const int BallsPerTurn = 3;
    public const int MinLineLength = 5;

    // Board: 0 = empty, 1-6 = ball colors
    public readonly int[ , ] Board = new int[ BoardSize, BoardSize ];

    // Next balls to appear (colors 1-6)
    public readonly int[] NextBalls = new int[ BallsPerTurn ];

    public int Score = 0;
    public bool IsGameOver = false;

    // Selection and movement
    public Vector2? SelectedCell = null;
    public Vector2? TargetCell = null;
    public List<Vector2> MovementPath = [ ];
    public int PathIndex = 0;
    public float MoveTimer = 0f;
    public const float MoveSpeed = 0.15f; // seconds per cell

    public bool IsAnimating => MovementPath.Count > 0 && PathIndex < MovementPath.Count;

    public void GenerateNextBalls()
    {
        Random random = new();
        for ( int i = 0; i < BallsPerTurn; i++ )
        {
            NextBalls[ i ] = random.Next( 1, ColorsCount + 1 );
        }
    }

    public int CountEmptyCells()
    {
        return GridHelper.CountEmptyCells( Board );
    }
}