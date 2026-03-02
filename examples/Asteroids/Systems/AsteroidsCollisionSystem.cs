using Aether.Core;
using Asteroids.Components;
using Graphics.Components;
using Graphics.Systems;
using Silk.NET.Maths;

namespace Asteroids.Systems;

public class AsteroidsCollisionSystem : SystemBase
{
    protected override void OnCreate() { }
    
    private Vector2D<float>[] GetTransformedVertices( Entity entity )
    {
        ref Collider collider = ref World.Get<Collider>( entity );
        ref Transform transform = ref World.Get<Transform>( entity );
        return TransformSystem.GetTransformedPolygon( collider.LocalVertices, transform );
    }

    private void RemoveEntities( List<Entity> entities )
    {
        foreach ( Entity entity in entities )
        {
            World.Despawn( entity );
        }
    }
    
    protected override void OnUpdate( float deltaTime )
    {
        GameStateSystem? gameStateSystem = World.GetSystem<GameStateSystem>();
        if ( gameStateSystem != null && gameStateSystem.IsGameOver() )
        {
            return;
        }

        CheckBulletAsteroidCollisions();
        CheckSpaceshipAsteroidCollisions();
    }

    protected override void OnRender() { }

    protected override void OnDestroy() { }
    
    private void CheckBulletAsteroidCollisions()
    {
        List<Entity> bulletsToRemove = [ ];

        foreach ( Entity bulletEntity in World.Filter<Bullet, Transform, Collider>() )
        {
            Vector2D<float>[] bulletVertices = GetTransformedVertices( bulletEntity );

            foreach ( Entity asteroidEntity in World.Filter<Asteroid, Transform, Collider>() )
            {
                Vector2D<float>[] asteroidVertices = GetTransformedVertices( asteroidEntity );

                if ( CollisionSystem.Gjk( bulletVertices, asteroidVertices ) )
                {
                    bulletsToRemove.Add( bulletEntity );
                    SplitAsteroid( asteroidEntity );
                    break;
                }
            }
        }

        RemoveEntities( bulletsToRemove );
    }
    
    private void CheckSpaceshipAsteroidCollisions()
    {
        List<Entity> shipsToRemove = [ ];

        foreach ( Entity spaceshipEntity in World.Filter<Spaceship, Transform, Collider>() )
        {
            Vector2D<float>[] spaceshipVertices = GetTransformedVertices( spaceshipEntity );

            foreach ( Entity asteroidEntity in World.Filter<Asteroid, Transform, Collider>() )
            {
                Vector2D<float>[] asteroidVertices = GetTransformedVertices( asteroidEntity );

                if ( CollisionSystem.Gjk( spaceshipVertices, asteroidVertices ) )
                {
                    HandleSpaceshipAsteroidCollision( spaceshipEntity, asteroidEntity );
                    shipsToRemove.Add( spaceshipEntity );
                    break;
                }
            }
        }

        DestroySpaceships( shipsToRemove );
    }
    
    private void HandleSpaceshipAsteroidCollision( Entity spaceshipEntity, Entity asteroidEntity )
    {
        ref Spaceship spaceship = ref World.Get<Spaceship>( spaceshipEntity );
        ref Transform spaceshipTransform = ref World.Get<Transform>( spaceshipEntity );

        SpawnExplosionEffect( spaceshipTransform.Position, spaceship.Velocity );
        SplitAsteroid( asteroidEntity );
    }
    
    private void SpawnExplosionEffect( Vector3D<float> position, Vector2D<float> velocity )
    {
        ParticleSystem? particleSystem = World.GetSystem<ParticleSystem>();
        particleSystem?.SpawnExplosion( position, velocity, new Vector4D<float>( 0.2f, 0.8f, 1f, 1f ), 12 );
    }
    
    private void SplitAsteroid( Entity asteroidEntity )
    {
        AsteroidSystem? asteroidSystem = World.GetSystem<AsteroidSystem>();
        asteroidSystem?.SplitAsteroid( asteroidEntity );
    }
    
    private void DestroySpaceships( List<Entity> shipsToRemove )
    {
        foreach ( Entity shipEntity in shipsToRemove )
        {
            World.Despawn( shipEntity );
            NotifyGameStateOfDeath();
        }
    }
    
    private void NotifyGameStateOfDeath()
    {
        GameStateSystem? gameStateSystem = World.GetSystem<GameStateSystem>();
        gameStateSystem?.LoseLife();
    }
}