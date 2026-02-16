using System.Numerics;
using Aether.Core.Systems;
using GameUtils.Helpers;

namespace Tetris.Systems;

public class GameSystem : SystemBase
{
    protected override void OnInit()
    {
        GameState state = World.GetGlobal<GameState>();
        SpawnNewPiece( state );
    }

    protected override void OnUpdate( float deltaTime )
    {
        GameState state = World.GetGlobal<GameState>();

        if ( state.IsGameOver )
            return;

        state.DropTimer += deltaTime;
        state.MoveTimer -= deltaTime;
        state.RotateTimer -= deltaTime;

        if ( state.DropTimer >= state.DropInterval )
        {
            state.DropTimer = 0f;

            if ( CanMovePiece( state, 0, -1 ) )
            {
                state.CurrentPosition.Y -= 1;
                state.IsLocking = false;
                state.LockTimer = 0f;
            }
            else
            {
                if ( !state.IsLocking )
                {
                    state.IsLocking = true;
                    state.LockTimer = 0f;
                }
            }
        }

        if ( state.IsLocking )
        {
            state.LockTimer += deltaTime;
            if ( state.LockTimer >= state.LockDelay )
            {
                LockPiece( state );
                ClearLines( state );
                SpawnNewPiece( state );
            }
        }
    }

    private void SpawnNewPiece( GameState state )
    {
        state.CurrentType = state.NextType;
        state.NextType = Tetromino.GetRandomType();
        state.CurrentRotation = 0;
        state.CurrentPosition = new Vector2( 4, 19 );
        state.IsLocking = false;
        state.LockTimer = 0f;

        if ( !CanMovePiece( state, 0, 0 ) )
        {
            state.IsGameOver = true;
        }
    }

    private bool CanMovePiece( GameState state, int dx, int dy )
    {
        Vector2[] blocks = Tetromino.GetBlocks( state.CurrentType, state.CurrentRotation );

        return !( from offset in blocks
            let x = ( int )( state.CurrentPosition.X + offset.X + dx )
            let y = ( int )( state.CurrentPosition.Y + offset.Y + dy )
            where state.IsOccupied( x, y )
            select x ).Any();
    }

    private bool CanRotatePiece( GameState state, int newRotation )
    {
        Vector2[] blocks = Tetromino.GetBlocks( state.CurrentType, newRotation );

        return !( from offset in blocks
            let x = ( int )( state.CurrentPosition.X + offset.X )
            let y = ( int )( state.CurrentPosition.Y + offset.Y )
            where state.IsOccupied( x, y )
            select x ).Any();
    }

    private void LockPiece( GameState state )
    {
        Vector2[] blocks = Tetromino.GetBlocks( state.CurrentType, state.CurrentRotation );
        int colorIndex = Tetromino.GetColorIndex( state.CurrentType );

        foreach ( Vector2 offset in blocks )
        {
            int x = ( int )( state.CurrentPosition.X + offset.X );
            int y = ( int )( state.CurrentPosition.Y + offset.Y );
            state.PlaceBlock( x, y, colorIndex );
        }
    }

    private void ClearLines( GameState state )
    {
        int linesCleared = GridHelper.ClearFullRows( state.Board );

        if ( linesCleared > 0 )
        {
            // Award points based on lines cleared
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

            // Level up every 10 lines
            int newLevel = state.LinesCleared / 10 + 1;
            if ( newLevel > state.Level )
            {
                state.Level = newLevel;
                state.DropInterval = Math.Max( 0.1f, 1.0f - ( state.Level - 1 ) * 0.08f );
            }
        }
    }

    public bool TryMovePiece( GameState state, int dx, int dy )
    {
        if ( CanMovePiece( state, dx, dy ) )
        {
            state.CurrentPosition.X += dx;
            state.CurrentPosition.Y += dy;

            if ( dy < 0 )
            {
                state.IsLocking = false;
                state.LockTimer = 0f;
            }

            return true;
        }

        return false;
    }

    public bool TryRotatePiece( GameState state )
    {
        int newRotation = ( state.CurrentRotation + 1 ) % 4;

        if ( CanRotatePiece( state, newRotation ) )
        {
            state.CurrentRotation = newRotation;
            state.IsLocking = false;
            state.LockTimer = 0f;
            return true;
        }

        return false;
    }

    public void HardDrop( GameState state )
    {
        while ( CanMovePiece( state, 0, -1 ) )
        {
            state.CurrentPosition.Y -= 1;
        }

        LockPiece( state );
        ClearLines( state );
        SpawnNewPiece( state );
    }
}