using System.Numerics;
using Aether.Core.Systems;
using GameUtils.Helpers;

namespace ColorLines.Systems;

public class GameSystem : SystemBase
{
    private readonly Random _random = new();

    protected override void OnInit()
    {
        GameState state = World.GetGlobal<GameState>();
        SpawnInitialBalls( state );
    }

    protected override void OnUpdate( float deltaTime )
    {
        GameState state = World.GetGlobal<GameState>();

        if ( state.IsGameOver )
            return;

        // Handle ball movement animation
        if ( state.IsAnimating )
        {
            state.MoveTimer += deltaTime;
            if ( state.MoveTimer >= GameState.MoveSpeed )
            {
                state.MoveTimer = 0f;
                state.PathIndex++;

                if ( state.PathIndex >= state.MovementPath.Count )
                {
                    // Movement complete
                    CompleteBallMove( state );
                }
            }
        }
    }

    private void SpawnInitialBalls( GameState state )
    {
        // Spawn 5 random balls at start
        for ( int i = 0; i < 5; i++ )
        {
            SpawnRandomBall( state );
        }
    }

    private void SpawnRandomBall( GameState state )
    {
        List<Vector2> emptyCells = GridHelper.GetEmptyCells( state.Board );

        if ( emptyCells.Count == 0 )
            return;

        Vector2 cell = emptyCells[ _random.Next( emptyCells.Count ) ];
        int color = _random.Next( 1, GameState.ColorsCount + 1 );
        state.Board[ ( int )cell.X, ( int )cell.Y ] = color;
    }

    public void StartBallMove( GameState state, Vector2 from, Vector2 to )
    {
        List<Vector2>? path = PathfindingHelper.FindPath( from, to, GameState.BoardSize, GameState.BoardSize,
            ( x, y ) => state.Board[ x, y ] == 0 || ( x == ( int )to.X && y == ( int )to.Y ) );

        if ( path == null || path.Count == 0 )
            return;

        state.MovementPath = path;
        state.PathIndex = 0;
        state.MoveTimer = 0f;
    }

    private void CompleteBallMove( GameState state )
    {
        if ( state.SelectedCell == null || state.TargetCell == null )
            return;

        Vector2 from = state.SelectedCell.Value;
        Vector2 to = state.TargetCell.Value;

        // Move ball
        int ballColor = state.Board[ ( int )from.X, ( int )from.Y ];
        state.Board[ ( int )from.X, ( int )from.Y ] = 0;
        state.Board[ ( int )to.X, ( int )to.Y ] = ballColor;

        // Check for lines
        bool linesRemoved = CheckAndRemoveLines( state, ( int )to.X, ( int )to.Y );

        // Spawn new balls if no lines were removed
        if ( !linesRemoved )
        {
            SpawnNextBalls( state );
            state.GenerateNextBalls();
        }

        // Clear selection
        state.SelectedCell = null;
        state.TargetCell = null;
        state.MovementPath.Clear();
        state.PathIndex = 0;

        // Check game over
        if ( state.CountEmptyCells() == 0 )
        {
            state.IsGameOver = true;
        }
    }

    private void SpawnNextBalls( GameState state )
    {
        List<Vector2> emptyCells = GridHelper.GetEmptyCells( state.Board );

        for ( int i = 0; i < GameState.BallsPerTurn && emptyCells.Count > 0; i++ )
        {
            int index = _random.Next( emptyCells.Count );
            Vector2 cell = emptyCells[ index ];
            emptyCells.RemoveAt( index );

            state.Board[ ( int )cell.X, ( int )cell.Y ] = state.NextBalls[ i ];

            // Check if this spawn created a line
            CheckAndRemoveLines( state, ( int )cell.X, ( int )cell.Y );
        }
    }

    private bool CheckAndRemoveLines( GameState state, int x, int y )
    {
        HashSet<Vector2> toRemove = LineDetectionHelper.FindMatchingLines(
            x, y, GameState.BoardSize, GameState.BoardSize,
            ( cx, cy ) => state.Board[ cx, cy ], GameState.MinLineLength );

        if ( toRemove.Count > 0 )
        {
            // Calculate score
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
            state.Score += points;

            // Remove balls
            foreach ( Vector2 cell in toRemove )
            {
                state.Board[ ( int )cell.X, ( int )cell.Y ] = 0;
            }

            return true;
        }

        return false;
    }

}