using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Utilities;
using Tetris.Components;

namespace Tetris.Systems;

public class TetrisLogicSystem : SystemBase
{
    protected override void OnInit()
    {
        Entity gameEntity = World.Spawn();

        World.Add( gameEntity, new TetrisBoardComponent
        {
            Board = new int[ 10, 20 ]
        } );
        World.Add( gameEntity, new TetrisPieceComponent
        {
            Type = TetrominoType.None,
            Rotation = 0,
            Position = new Vector2D<float>( 4, 19 )
        } );
        World.Add( gameEntity, new TetrisGameStateComponent
        {
            Score = 0,
            Level = 1,
            LinesCleared = 0,
            IsGameOver = false,
            NextType = Tetromino.GetRandomType()
        } );
        World.Add( gameEntity, new TetrisTimerComponent
        {
            DropTimer = 0f,
            DropInterval = 1.0f,
            LockTimer = 0f,
            IsLocking = false,
            MoveTimer = 0f,
            RotateTimer = 0f
        } );

        // Spawn first piece
        ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>( gameEntity );
        ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>( gameEntity );
        ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>( gameEntity );
        ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( gameEntity );

        SpawnNewPiece( ref state, ref piece, ref timers, board.Board );
    }

    protected override void OnUpdate( float deltaTime )
    {
        foreach ( Entity entity in World.Filter<TetrisGameStateComponent>().With<TetrisTimerComponent>() )
        {
            if ( !World.Has<TetrisPieceComponent>( entity ) || !World.Has<TetrisBoardComponent>( entity ) )
                continue;

            ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>( entity );
            ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>( entity );
            ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>( entity );
            ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( entity );

            if ( state.IsGameOver )
                return;

            timers.DropTimer += deltaTime;
            timers.MoveTimer -= deltaTime;
            timers.RotateTimer -= deltaTime;

            if ( timers.DropTimer >= timers.DropInterval )
            {
                timers.DropTimer = 0f;

                if ( CanMovePiece( board.Board, piece, 0, -1 ) )
                {
                    piece.Position = new Vector2D<float>( piece.Position.X, piece.Position.Y - 1 );
                    timers.IsLocking = false;
                    timers.LockTimer = 0f;
                }
                else
                {
                    if ( !timers.IsLocking )
                    {
                        timers.IsLocking = true;
                        timers.LockTimer = 0f;
                    }
                }
            }

            if ( timers.IsLocking )
            {
                timers.LockTimer += deltaTime;
                if ( timers.LockTimer >= 0.5f ) // LockDelay
                {
                    LockPiece( board.Board, piece );
                    ClearLines( ref state, ref timers, board.Board );
                    SpawnNewPiece( ref state, ref piece, ref timers, board.Board );
                }
            }
        }
    }

    public void ResetGame()
    {
        foreach ( Entity entity in World.Filter<TetrisGameStateComponent>() )
        {
            if ( !World.Has<TetrisTimerComponent>( entity ) || !World.Has<TetrisPieceComponent>( entity ) ||
                 !World.Has<TetrisBoardComponent>( entity ) )
                continue;

            ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>( entity );
            ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>( entity );
            ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>( entity );
            ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( entity );

            state.Score = 0;
            state.Level = 1;
            state.LinesCleared = 0;
            state.IsGameOver = false;
            state.NextType = Tetromino.GetRandomType();

            timers.DropInterval = 1.0f;
            timers.DropTimer = 0f;
            timers.LockTimer = 0f;
            timers.IsLocking = false;
            timers.MoveTimer = 0f;
            timers.RotateTimer = 0f;

            // Clear board
            for ( int x = 0; x < 10; x++ )
            for ( int y = 0; y < 20; y++ )
                board.Board[ x, y ] = 0;

            SpawnNewPiece( ref state, ref piece, ref timers, board.Board );
        }
    }

    private void SpawnNewPiece( ref TetrisGameStateComponent state, ref TetrisPieceComponent piece,
        ref TetrisTimerComponent timers, int[ , ] board )
    {
        piece.Type = state.NextType;
        state.NextType = Tetromino.GetRandomType();
        piece.Rotation = 0;
        piece.Position = new Vector2D<float>( 4, 19 );
        timers.IsLocking = false;
        timers.LockTimer = 0f;

        if ( !CanMovePiece( board, piece, 0, 0 ) )
        {
            state.IsGameOver = true;
        }
    }

    private bool CanMovePiece( int[ , ] board, TetrisPieceComponent piece, int dx, int dy )
    {
        Vector2D<float>[] blocks = Tetromino.GetBlocks( piece.Type, piece.Rotation );

        foreach ( Vector2D<float> offset in blocks )
        {
            int x = ( int )( piece.Position.X + offset.X + dx );
            int y = ( int )( piece.Position.Y + offset.Y + dy );

            if ( IsOccupied( board, x, y ) )
                return false;
        }

        return true;
    }

    private bool CanRotatePiece( int[ , ] board, TetrisPieceComponent piece, int newRotation )
    {
        Vector2D<float>[] blocks = Tetromino.GetBlocks( piece.Type, newRotation );

        foreach ( Vector2D<float> offset in blocks )
        {
            int x = ( int )( piece.Position.X + offset.X );
            int y = ( int )( piece.Position.Y + offset.Y );

            if ( IsOccupied( board, x, y ) )
                return false;
        }

        return true;
    }

    private bool IsOccupied( int[ , ] board, int x, int y )
    {
        if ( x is < 0 or >= 10 ) return true;
        if ( y < 0 ) return true;
        if ( y >= 20 ) return false;
        return board[ x, y ] != 0;
    }

    private void LockPiece( int[ , ] board, TetrisPieceComponent piece )
    {
        Vector2D<float>[] blocks = Tetromino.GetBlocks( piece.Type, piece.Rotation );
        int colorIndex = Tetromino.GetColorIndex( piece.Type );

        foreach ( Vector2D<float> offset in blocks )
        {
            int x = ( int )( piece.Position.X + offset.X );
            int y = ( int )( piece.Position.Y + offset.Y );
            if ( x is >= 0 and < 10 && y is >= 0 and < 20 )
            {
                board[ x, y ] = colorIndex;
            }
        }
    }

    private void ClearLines( ref TetrisGameStateComponent state, ref TetrisTimerComponent timers, int[ , ] board )
    {
        int linesCleared = GridHelper.ClearFullRows( board );

        if ( linesCleared > 0 )
        {
            int points = linesCleared switch
            {
                1 => 10,
                2 => 30,
                3 => 70,
                4 => 150,
                _ => 0
            };

            state.Score += points;
            state.LinesCleared += linesCleared;

            int newLevel = state.LinesCleared / 10 + 1;
            if ( newLevel > state.Level )
            {
                state.Level = newLevel;
                timers.DropInterval = Math.Max( 0.1f, 1.0f - ( state.Level - 1 ) * 0.08f );
            }
        }
    }

    public void TryMovePiece( Entity entity, int dx, int dy )
    {
        ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>( entity );
        ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( entity );
        ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>( entity );

        if ( CanMovePiece( board.Board, piece, dx, dy ) )
        {
            piece.Position = new Vector2D<float>( piece.Position.X + dx, piece.Position.Y + dy );

            if ( dy < 0 )
            {
                timers.IsLocking = false;
                timers.LockTimer = 0f;
            }
        }
    }

    public void TryRotatePiece( Entity entity )
    {
        ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>( entity );
        ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( entity );
        ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>( entity );

        int newRotation = ( piece.Rotation + 1 ) % 4;

        if ( CanRotatePiece( board.Board, piece, newRotation ) )
        {
            piece.Rotation = newRotation;
            timers.IsLocking = false;
            timers.LockTimer = 0f;
        }
    }

    public void HardDrop( Entity entity )
    {
        ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>( entity );
        ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( entity );
        ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>( entity );
        ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>( entity );

        while ( CanMovePiece( board.Board, piece, 0, -1 ) )
        {
            piece.Position = new Vector2D<float>( piece.Position.X, piece.Position.Y - 1 );
        }

        LockPiece( board.Board, piece );
        ClearLines( ref state, ref timers, board.Board );
        SpawnNewPiece( ref state, ref piece, ref timers, board.Board );
    }
}