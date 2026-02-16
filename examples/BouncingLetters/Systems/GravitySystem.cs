using Aether.Core;
using Aether.Core.Systems;
using BouncingLetters.Components;
using Graphics.Components;

namespace BouncingLetters.Systems;

public class GravitySystem : SystemBase
{
    private const float _gravity = -9.81f;
    private const float _groundY = -3f;

    protected override void OnUpdate( float deltaTime )
    {
        foreach ( Entity entity in World.Filter<Physics>().With<Transform>() )
        {
            ref Physics physics = ref World.Get<Physics>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            physics.Velocity.Y += _gravity * deltaTime;

            float newY = transform.Position.Y + physics.Velocity.Y * deltaTime;

            if ( newY <= _groundY && physics.Velocity.Y < 0 )
            {
                float timeToGround = ( _groundY - transform.Position.Y ) / physics.Velocity.Y;
                float velocityAtGround = physics.Velocity.Y + _gravity * timeToGround;

                physics.Velocity.Y = -velocityAtGround * physics.Bounciness;
                transform.Position.Y = _groundY;
            }
            else
            {
                transform.Position.Y = newY;
            }
        }
    }
}