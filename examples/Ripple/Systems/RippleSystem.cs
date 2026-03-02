using Aether.Core;
using Silk.NET.Input;
using Graphics.Systems;
using Ripple.Components;

namespace Ripple.Systems;

public class RippleSystem : SystemBase
{
    private InputSystem? _inputSystem;
    private Graphics.Components.Input? _input;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Graphics.Components.Input>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem is null || _input is null )
            return;

        foreach ( Entity entity in World.Filter<Components.Ripple>() )
        {
            ref Components.Ripple ripple = ref World.Get<Components.Ripple>( entity );

            HandleInput( ref ripple );
            UpdateRippleTime( ref ripple, deltaTime );
        }
    }

    private void HandleInput( ref Components.Ripple ripple )
    {
        if ( _inputSystem!.IsKeyPressed( _input!, Key.R ) )
        {
            ripple.Time = 0f;
            ripple.IsPlaying = true;
            ripple.IsForward = !ripple.IsForward;
        }
    }

    private void UpdateRippleTime( ref Components.Ripple ripple, float deltaTime )
    {
        if ( !ripple.IsPlaying )
            return;

        ripple.Time += deltaTime;

        if ( ripple.Time >= ripple.Duration )
        {
            ripple.Time = ripple.Duration;
            ripple.IsPlaying = false;
        }
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }
}