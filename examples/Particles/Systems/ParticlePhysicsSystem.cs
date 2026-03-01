using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Particles.Components;

namespace Particles.Systems;

public class ParticlePhysicsSystem : SystemBase
{
    // Физические константы
    private const float _coulombConstant = 50.0f; // Коэффициент силы Кулона
    private const float _minDistance = 1f; // Минимальное расстояние для отталкивания
    private const float _repulsionForce = 10.0f; // Сила отталкивания при близком контакте
    private const float _damping = 0.98f; // Коэффициент затухания для стабилизации

    // Границы пространства
    private const float _boundaryX = 18.0f;
    private const float _boundaryY = 13.0f;
    private const float _boundaryRestitution = 0.8f; // Коэффициент упругости стенок

    private bool _initialized;

    protected override void OnUpdate( float deltaTime )
    {
        if ( !_initialized )
        {
            InitializeParticles();
            _initialized = true;
        }

        // Ограничиваем deltaTime для стабильности
        deltaTime = Math.Min( deltaTime, 0.016f );

        List<Entity> particles = [ ];
        foreach ( Entity e in World.Filter<ChargedParticle>().With<Transform>() )
        {
            particles.Add( e );
        }

        int count = particles.Count;

        // Вычисляем силы между всеми парами частиц
        for ( int i = 0; i < count; i++ )
        {
            Entity entityA = particles[ i ];
            ref ChargedParticle particleA = ref World.Get<ChargedParticle>( entityA );
            ref Transform transformA = ref World.Get<Transform>( entityA );

            Vector2D<float> totalForce = new( 0, 0 );

            for ( int j = 0; j < count; j++ )
            {
                if ( i == j ) continue;

                Entity entityB = particles[ j ];
                ref ChargedParticle particleB = ref World.Get<ChargedParticle>( entityB );
                ref Transform transformB = ref World.Get<Transform>( entityB );

                Vector2D<float> posA = new( transformA.Position.X, transformA.Position.Y );
                Vector2D<float> posB = new( transformB.Position.X, transformB.Position.Y );

                Vector2D<float> direction = posB - posA;
                float distance = direction.Length;

                if ( distance < 0.01f ) continue;

                Vector2D<float> normalized = Vector2D.Normalize( direction );

                // Сила Кулона: F = k * q1 * q2 / r^2
                float coulombForce = _coulombConstant * particleA.Charge * particleB.Charge / ( distance * distance );

                // Сила отталкивания при близком контакте
                if ( distance < _minDistance )
                {
                    float repulsion = _repulsionForce * ( _minDistance - distance ) / _minDistance;
                    coulombForce -= repulsion; // Отталкивание (всегда отрицательное)
                }

                totalForce -= normalized * coulombForce; // Минус, т.к. одноименные заряды отталкиваются
            }

            // Применяем силу: F = ma => a = F/m
            Vector2D<float> acceleration = totalForce / particleA.Mass;
            particleA.Velocity += acceleration * deltaTime;

            // Применяем затухание
            particleA.Velocity *= _damping;
        }

        // Обновляем позиции и проверяем границы
        foreach ( Entity entity in particles )
        {
            ref ChargedParticle particle = ref World.Get<ChargedParticle>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            // Обновляем позицию
            Vector2D<float> pos = new( transform.Position.X, transform.Position.Y );
            pos += particle.Velocity * deltaTime;

            // Проверяем границы и отражаем
            if ( pos.X - particle.Radius < -_boundaryX )
            {
                pos.X = -_boundaryX + particle.Radius;
                particle.Velocity.X = -particle.Velocity.X * _boundaryRestitution;
            }
            else if ( pos.X + particle.Radius > _boundaryX )
            {
                pos.X = _boundaryX - particle.Radius;
                particle.Velocity.X = -particle.Velocity.X * _boundaryRestitution;
            }

            if ( pos.Y - particle.Radius < -_boundaryY )
            {
                pos.Y = -_boundaryY + particle.Radius;
                particle.Velocity.Y = -particle.Velocity.Y * _boundaryRestitution;
            }
            else if ( pos.Y + particle.Radius > _boundaryY )
            {
                pos.Y = _boundaryY - particle.Radius;
                particle.Velocity.Y = -particle.Velocity.Y * _boundaryRestitution;
            }

            transform.Position = new Vector3D<float>( pos.X, pos.Y, 0 );
        }
    }

    private void InitializeParticles()
    {
        Random random = new( 42 );
        int particleCount = 25;
        int positiveCount = particleCount / 2;
        int negativeCount = particleCount - positiveCount;

        // Создаем положительные частицы (красные)
        for ( int i = 0; i < positiveCount; i++ )
        {
            CreateParticle( random, charge: 1.0f, isPositive: true );
        }

        // Создаем отрицательные частицы (синие)
        for ( int i = 0; i < negativeCount; i++ )
        {
            CreateParticle( random, charge: -1.0f, isPositive: false );
        }
    }

    private void CreateParticle( Random random, float charge, bool isPositive )
    {
        Entity entity = World.Spawn();

        // Случайная позиция в пределах границ
        float x = ( float )( random.NextDouble() * 2 - 1 ) * ( _boundaryX - 2 );
        float y = ( float )( random.NextDouble() * 2 - 1 ) * ( _boundaryY - 2 );

        Transform transform = new()
        {
            Position = new Vector3D<float>( x, y, 0 ),
            Rotation = Quaternion<float>.Identity,
            Scale = new Vector3D<float>( 1, 1, 1 )
        };

        ChargedParticle particle = new()
        {
            Color = isPositive
                ? new Vector4D<float>( 1.0f, 0.2f, 0.2f, 1.0f )
                : new Vector4D<float>( 0.2f, 0.4f, 1.0f, 1.0f ),
            Charge = charge,
            Mass = 1.0f,
            Velocity = new Vector2D<float>( 0, 0 ),
            Radius = 1.0f  // Увеличили с 0.4 до 1.0
        };

        World.Add( entity, transform );
        World.Add( entity, particle );
    }
}