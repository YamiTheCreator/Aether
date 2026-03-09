using Aether.Core;
using Asteroids.Builders;
using Asteroids.Components;
using Graphics.Components;
using Graphics.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Asteroids.Systems;

public class GameStateSystem : SystemBase
{
    private InputSystem? _inputSystem;
    private Input? _input;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();

        GameState gameState = new()
        {
            Lives = 3,
            Score = 0,
            Wave = 1,
            IsGameOver = false,
            RespawnTimer = 0
        };

        World.Spawn( gameState );

        RespawnPlayer();
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem == null || _input == null )
            return;

        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );

            if ( gameState.IsGameOver && _inputSystem.IsKeyPressed( _input, Key.R ) )
            {
                RestartGame();
                return;
            }

            if ( gameState.IsGameOver )
            {
                break;
            }

            if ( gameState.RespawnTimer > 0 )
            {
                gameState.RespawnTimer -= deltaTime;

                if ( gameState.RespawnTimer <= 0 )
                {
                    if ( !IsPlayerAlive() )
                    {
                        RespawnPlayer();
                    }
                }
            }

            break;
        }
    }

    protected override void OnRender() { }

    protected override void OnDestroy() { }

    public void AddScore( int points )
    {
        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            gameState.Score += points;
            break;
        }
    }

    public void LoseLife()
    {
        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            gameState.Lives--;

            if ( gameState.Lives <= 0 )
            {
                gameState.IsGameOver = true;
            }
            else
            {
                gameState.RespawnTimer = 2f;
            }

            break;
        }
    }

    public bool IsPlayerAlive()
    {
        foreach ( Entity entity in World.Filter<Spaceship>() )
        {
            return true;
        }

        return false;
    }

    public bool IsGameOver()
    {
        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            return gameState.IsGameOver;
        }

        return false;
    }

    public int GetLives()
    {
        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            return gameState.Lives;
        }

        return 0;
    }

    public int GetScore()
    {
        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            return gameState.Score;
        }

        return 0;
    }

    public int GetWaveNumber()
    {
        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            return gameState.Wave;
        }

        return 1;
    }

    public void NextWave()
    {
        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            gameState.Wave++;
            break;
        }
    }

    private void RespawnPlayer()
    {
        if ( !IsSafeToRespawn() )
        {
            foreach ( Entity gameStateEntity in World.Filter<GameState>() )
            {
                ref GameState gameState = ref World.Get<GameState>( gameStateEntity );
                gameState.RespawnTimer = 0.5f;
                break;
            }

            return;
        }

        Spaceship spaceship = new()
        {
            Velocity = Vector2D<float>.Zero,
            AngularVelocity = 0f,
            ShootCooldown = 0f
        };

        Collider collider = new()
        {
            LocalVertices = EntityBuilder.CreateSpaceshipVertices()
        };

        Graphics.Components.Transform transform = new( Vector3D<float>.Zero )
        {
            Scale = new Vector3D<float>( 0.8f, 0.8f, 1f ),
            Rotation = Quaternion<float>.Identity
        };

        Entity entity = World.Spawn( spaceship );
        World.Add( entity, transform );
        World.Add( entity, collider );
    }

    private bool IsSafeToRespawn()
    {
        Vector3D<float> center = Vector3D<float>.Zero;
        const float safeRadius = 4f;

        Graphics.Components.Transform spaceshipTransform = new( center )
        {
            Scale = new Vector3D<float>( 0.8f, 0.8f, 1f ),
            Rotation = Quaternion<float>.Identity
        };

        Vector2D<float>[] spaceshipVertices = TransformSystem.GetTransformedPolygon(
            EntityBuilder.CreateSpaceshipVertices(),
            spaceshipTransform
        );

        foreach ( Entity asteroidEntity in World.Filter<Asteroid>() )
        {
            ref Collider collider = ref World.Get<Collider>( asteroidEntity );
            ref Graphics.Components.Transform asteroidTransform =
                ref World.Get<Graphics.Components.Transform>( asteroidEntity );
            Vector3D<float> asteroidPos = asteroidTransform.Position;

            float dx = center.X - asteroidPos.X;
            float dy = center.Y - asteroidPos.Y;
            float distance = MathF.Sqrt( dx * dx + dy * dy );

            if ( distance > safeRadius )
                continue;

            Vector2D<float>[] asteroidVertices = TransformSystem.GetTransformedPolygon(
                collider.LocalVertices,
                asteroidTransform
            );

            if ( CollisionSystem.Gjk( spaceshipVertices, asteroidVertices ) )
            {
                return false;
            }
        }

        return true;
    }

    public void RestartGame()
    {
        List<Entity> asteroidsToRemove = [ ];
        foreach ( Entity asteroidEntity in World.Filter<Asteroid>() )
        {
            asteroidsToRemove.Add( asteroidEntity );
        }

        foreach ( Entity asteroidEntity in asteroidsToRemove )
        {
            World.Despawn( asteroidEntity );
        }

        List<Entity> bulletsToRemove = [ ];
        foreach ( Entity bulletEntity in World.Filter<Bullet>() )
        {
            bulletsToRemove.Add( bulletEntity );
        }

        foreach ( Entity bulletEntity in bulletsToRemove )
        {
            World.Despawn( bulletEntity );
        }

        List<Entity> particlesToRemove = [ ];
        foreach ( Entity particleEntity in World.Filter<Particle>() )
        {
            particlesToRemove.Add( particleEntity );
        }

        foreach ( Entity particleEntity in particlesToRemove )
        {
            World.Despawn( particleEntity );
        }

        foreach ( Entity entity in World.Filter<GameState>() )
        {
            ref GameState gameState = ref World.Get<GameState>( entity );
            gameState.Lives = 3;
            gameState.Score = 0;
            gameState.Wave = 1;
            gameState.IsGameOver = false;
            gameState.RespawnTimer = 0;
            break;
        }

        AsteroidSystem? asteroidSystem = World.GetSystem<AsteroidSystem>();
        if ( asteroidSystem != null )
        {
            for ( int i = 0; i < 5; i++ )
            {
                asteroidSystem.SpawnAsteroid( 3 );
            }
        }

        RespawnPlayer();
    }
}