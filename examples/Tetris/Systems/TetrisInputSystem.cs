using Aether.Core;
using Graphics;
using Silk.NET.Input;
using Tetris.Components;

namespace Tetris.Systems;

public class TetrisInputSystem : SystemBase
{
    private TetrisLogicSystem? _logicSystem;
    private const float _moveDelay = 0.15f;
    private const float _rotateDelay = 0.2f;

    private readonly HashSet<Key> _pressedKeys = [ ];

    protected override void OnUpdate( float deltaTime )
    {
        _logicSystem ??= World.GetSystem<TetrisLogicSystem>();
        if ( _logicSystem == null )
            return;

        IKeyboard keyboard = WindowBase.Input.Keyboards[ 0 ];

        foreach ( Entity entity in World.Filter<TetrisGameStateComponent>().With<TetrisTimerComponent>() )
        {
            if ( !World.Has<TetrisPieceComponent>( entity ) || !World.Has<TetrisBoardComponent>( entity ) )
                continue;

            ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>( entity );
            ref TetrisTimerComponent timers = ref World.Get<TetrisTimerComponent>( entity );

            if ( state.IsGameOver )
            {
                if ( IsKeyPressed( keyboard, Key.R ) )
                {
                    _logicSystem.ResetGame();
                }

                return;
            }

            if ( IsKeyPressed( keyboard, Key.Left ) ||
                 ( keyboard.IsKeyPressed( Key.Left ) && timers.MoveTimer <= 0 ) )
            {
                _logicSystem.TryMovePiece( entity, -1, 0 );
                timers.MoveTimer = _moveDelay;
            }

            if ( IsKeyPressed( keyboard, Key.Right ) ||
                 ( keyboard.IsKeyPressed( Key.Right ) && timers.MoveTimer <= 0 ) )
            {
                _logicSystem.TryMovePiece( entity, 1, 0 );
                timers.MoveTimer = _moveDelay;
            }

            if ( keyboard.IsKeyPressed( Key.Down ) )
            {
                timers.DropTimer += deltaTime * 10f;
            }

            if ( ( IsKeyPressed( keyboard, Key.Up ) || IsKeyPressed( keyboard, Key.Z ) ) &&
                 timers.RotateTimer <= 0 )
            {
                _logicSystem.TryRotatePiece( entity );
                timers.RotateTimer = _rotateDelay;
            }

            // Hard drop
            if ( IsKeyPressed( keyboard, Key.Space ) )
            {
                _logicSystem.HardDrop( entity );
            }
        }

        UpdatePressedKeys( keyboard );
    }

    private bool IsKeyPressed( IKeyboard keyboard, Key key )
    {
        bool isDown = keyboard.IsKeyPressed( key );
        bool wasPressed = _pressedKeys.Contains( key );

        return isDown && !wasPressed;
    }

    private void UpdatePressedKeys( IKeyboard keyboard )
    {
        _pressedKeys.Clear();
        foreach ( Key key in keyboard.SupportedKeys )
        {
            if ( keyboard.IsKeyPressed( key ) )
            {
                _pressedKeys.Add( key );
            }
        }
    }
}