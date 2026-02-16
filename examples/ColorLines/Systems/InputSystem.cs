using System.Numerics;
using Aether.Core;
using Aether.Core.Systems;
using GameUtils.Helpers;
using Graphics.Components;
using Graphics.Input;
using Silk.NET.Input;

namespace ColorLines.Systems;

public class InputSystem : SystemBase
{
    private GameSystem? _gameSystem;
    private bool _previousMouseState;

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

        if ( state.IsAnimating )
            return;

        // Check for mouse click
        if ( InputHelper.IsMouseJustPressed( MouseButton.Left, ref _previousMouseState ) )
        {
            Vector2? cell = GetCellFromMouse();
            if ( cell == null )
                return;

            int x = ( int )cell.Value.X;
            int y = ( int )cell.Value.Y;

            // If no ball selected, select this cell if it has a ball
            if ( state.SelectedCell == null )
            {
                if ( state.Board[ x, y ] != 0 )
                {
                    state.SelectedCell = cell;
                }
            }
            else
            {
                // If clicking same cell, deselect
                if ( state.SelectedCell.Value.X == x && state.SelectedCell.Value.Y == y )
                {
                    state.SelectedCell = null;
                }
                // If clicking another ball, select it instead
                else if ( state.Board[ x, y ] != 0 )
                {
                    state.SelectedCell = cell;
                }
                // If clicking empty cell, try to move
                else if ( state.Board[ x, y ] == 0 )
                {
                    state.TargetCell = cell;
                    _gameSystem.StartBallMove( state, state.SelectedCell.Value, cell.Value );
                }
            }
        }
    }

    private Vector2? GetCellFromMouse()
    {
        Vector2 mousePos = InputHelper.GetMousePosition();

        Camera camera = default;
        foreach ( Entity e in World.Filter<Camera>() )
        {
            camera = World.Get<Camera>( e );
            break;
        }

        Vector2 worldPos = InputHelper.ScreenToWorld( mousePos, camera );

        const float boardStartX = -9f;
        const float boardStartY = -9f;
        const float cellSize = 1.8f;

        return InputHelper.WorldToCell( worldPos, new Vector2( boardStartX, boardStartY ), cellSize,
            GameState.BoardSize, GameState.BoardSize );
    }

    private void RestartGame( GameState state )
    {
        // Clear board
        for ( int x = 0; x < GameState.BoardSize; x++ )
        {
            for ( int y = 0; y < GameState.BoardSize; y++ )
            {
                state.Board[ x, y ] = 0;
            }
        }

        state.Score = 0;
        state.IsGameOver = false;
        state.SelectedCell = null;
        state.TargetCell = null;
        state.MovementPath.Clear();
        state.PathIndex = 0;
        state.GenerateNextBalls();

        // Spawn initial balls
        Random random = new();
        for ( int i = 0; i < 5; i++ )
        {
            List<Vector2> emptyCells = GridHelper.GetEmptyCells( state.Board );

            if ( emptyCells.Count > 0 )
            {
                Vector2 cell = emptyCells[ random.Next( emptyCells.Count ) ];
                int color = random.Next( 1, GameState.ColorsCount + 1 );
                state.Board[ ( int )cell.X, ( int )cell.Y ] = color;
            }
        }
    }
}