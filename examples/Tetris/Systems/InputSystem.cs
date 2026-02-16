using Aether.Core.Systems;
using Graphics.Input;
using Silk.NET.Input;

namespace Tetris.Systems;

public class InputSystem : SystemBase
{
    private GameSystem? _gameSystem;

    protected override void OnInit()
    {
        _gameSystem = null;
        foreach ( SystemBase system in World.GetType().GetField( "_updateSystems",
                         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance )
                     ?.GetValue( World ) as List<SystemBase> ?? [ ] )
        {
            if ( system is GameSystem gameSystem )
            {
                _gameSystem = gameSystem;
                break;
            }
        }
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( _gameSystem == null )
            return;

        GameState state = World.GetGlobal<GameState>();

        if ( state.IsGameOver )
        {
            if ( Input.IsKeyPressed( Key.R ) )
            {
                RestartGame( state );
            }

            return;
        }

        if ( Input.IsKeyPressed( Key.Left ) || ( Input.IsKeyDown( Key.Left ) && state.MoveTimer <= 0 ) )
        {
            if ( _gameSystem.TryMovePiece( state, -1, 0 ) )
            {
                state.MoveTimer = GameState.MoveDelay;
            }
        }

        if ( Input.IsKeyPressed( Key.Right ) || ( Input.IsKeyDown( Key.Right ) && state.MoveTimer <= 0 ) )
        {
            if ( _gameSystem.TryMovePiece( state, 1, 0 ) )
            {
                state.MoveTimer = GameState.MoveDelay;
            }
        }

        if ( Input.IsKeyDown( Key.Down ) )
        {
            state.DropTimer += deltaTime * 10f;
        }

        if ( ( Input.IsKeyPressed( Key.Up ) || Input.IsKeyPressed( Key.Z ) ) && state.RotateTimer <= 0 )
        {
            if ( _gameSystem.TryRotatePiece( state ) )
            {
                state.RotateTimer = GameState.RotateDelay;
            }
        }

        if ( Input.IsKeyPressed( Key.Space ) )
        {
            _gameSystem.HardDrop( state );
        }
    }

    private void RestartGame( GameState state )
    {
        for ( int x = 0; x < GameState.Width; x++ )
        {
            for ( int y = 0; y < GameState.Height; y++ )
            {
                state.Board[ x, y ] = 0;
            }
        }

        state.IsGameOver = false;
        state.Level = 1;
        state.LinesCleared = 0;
        state.Score = 0;
        state.DropInterval = 1.0f;
        state.DropTimer = 0f;
        state.IsLocking = false;
        state.LockTimer = 0f;

        state.NextType = Tetromino.GetRandomType();
        state.CurrentType = Tetromino.GetRandomType();
        state.CurrentRotation = 0;
        state.CurrentPosition = new System.Numerics.Vector2( 4, 19 );
    }
}