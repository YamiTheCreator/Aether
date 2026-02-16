using System.Numerics;
using Aether.Core;
using Aether.Core.Systems;
using GameUtils.Helpers;
using Graphics.Components;
using Graphics.Input;
using Silk.NET.Input;

namespace Hangman.Systems;

public class InputSystem : SystemBase
{
    private HangmanState _state = null!;
    private bool _wasMouseDown;

    protected override void OnInit()
    {
        _state = World.GetGlobal<HangmanState>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( _state.IsGameOver )
        {
            if ( Input.IsKeyPressed( Key.R ) )
            {
                _state.Reset();
            }

            return;
        }

        if ( InputHelper.IsMouseJustPressed( MouseButton.Left, ref _wasMouseDown ) )
        {
            Vector2 mousePos = InputHelper.GetMousePosition();

            Camera camera = default;
            foreach ( Entity e in World.Filter<Camera>() )
            {
                camera = World.Get<Camera>( e );
                break;
            }

            Vector2 worldPos = InputHelper.ScreenToWorld( mousePos, camera );

            foreach ( Entity entity in World.Filter<UI.Components.Button>() )
            {
                ref UI.Components.Button button = ref World.Get<UI.Components.Button>( entity );

                if ( button.Contains( worldPos.X, worldPos.Y ) && !string.IsNullOrEmpty( button.Text ) )
                {
                    _state.GuessLetter( button.Text[ 0 ] );
                    break;
                }
            }
        }
    }
}