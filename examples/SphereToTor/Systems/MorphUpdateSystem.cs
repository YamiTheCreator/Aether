using Aether.Core;
using Silk.NET.Input;
using Graphics.Systems;
using SphereToTor.Components;

namespace SphereToTor.Systems;

public class MorphUpdateSystem : SystemBase
{
    private readonly InputSystem _inputSystem = new();

    protected override void OnUpdate( float deltaTime )
    {
        if ( !World.HasGlobal<Graphics.Components.Input>() )
            return;

        Graphics.Components.Input input = World.GetGlobal<Graphics.Components.Input>();

        foreach ( Entity entity in World.Filter<MorphComponent>() )
        {
            ref MorphComponent morph = ref World.Get<MorphComponent>( entity );

            if ( _inputSystem.IsKeyPressed( input, Key.R ) )
            {
                morph.Time = 0f;
                morph.IsPlaying = true;
                morph.IsForward = !morph.IsForward;

                string direction = morph.IsForward ? "Sphere → Torus" : "Torus → Sphere";
            }

            if ( morph.IsPlaying )
            {
                morph.Time += deltaTime;

                if ( morph.Time >= morph.Duration )
                {
                    morph.Time = morph.Duration;
                    morph.IsPlaying = false;
                }
            }
        }
    }
}