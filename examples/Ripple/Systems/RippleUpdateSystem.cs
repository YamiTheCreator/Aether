using Aether.Core;
using Silk.NET.Input;
using Graphics.Systems;
using Ripple.Components;

namespace Ripple.Systems;

public class RippleUpdateSystem : SystemBase
{
    private readonly InputSystem _inputSystem = new();

    protected override void OnUpdate( float deltaTime )
    {
        if ( !World.HasGlobal<Graphics.Components.Input>() )
            return;

        Graphics.Components.Input input = World.GetGlobal<Graphics.Components.Input>();

        foreach ( Entity entity in World.Filter<RippleComponent>() )
        {
            ref RippleComponent ripple = ref World.Get<RippleComponent>( entity );

            if ( _inputSystem.IsKeyPressed( input, Key.R ) )
            {
                ripple.Time = 0f;
                ripple.IsPlaying = true;
                ripple.IsForward = !ripple.IsForward;
            }

            if ( ripple.IsPlaying )
            {
                ripple.Time += deltaTime;

                if ( ripple.Time >= ripple.Duration )
                {
                    ripple.Time = ripple.Duration;
                    ripple.IsPlaying = false;
                }
            }
        }
    }
}