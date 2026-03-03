using Aether.Core;
using Asteroids.Components;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Asteroids.Systems;

public class SpaceshipSystem : SystemBase
{
    private GL? _gl;
    private InputSystem? _inputSystem;
    private Input? _input;

    protected override void OnCreate()
    {
        _gl = World.GetGlobal<GL>();
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        SpaceshipConfig config = GetConfig();
        WorldBounds bounds = GetWorldBounds();

        GameStateSystem? gameStateSystem = World.GetSystem<GameStateSystem>();
        if ( gameStateSystem != null && gameStateSystem.IsGameOver() )
        {
            return;
        }

        foreach ( Entity entity in World.Filter<Spaceship>() )
        {
            ref Spaceship spaceship = ref World.Get<Spaceship>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            HandleInput( ref spaceship, ref transform, config, deltaTime );

            if ( spaceship.ShootCooldown > 0 )
            {
                spaceship.ShootCooldown -= deltaTime;
            }

            spaceship.AngularVelocity *= config.AngularDrag;

            float currentAngle = GetAngleFromQuaternion( transform.Rotation );
            currentAngle += spaceship.AngularVelocity * deltaTime;

            transform.Rotation = Quaternion<float>.CreateFromAxisAngle( Vector3D<float>.UnitZ, currentAngle );

            spaceship.Velocity *= config.LinearDrag;

            transform.Position.X += spaceship.Velocity.X * deltaTime;
            transform.Position.Y += spaceship.Velocity.Y * deltaTime;

            WrapPosition( ref transform.Position, bounds );
        }
    }

    protected override void OnRender()
    {
        if ( _gl == null ) return;

        foreach ( Entity entity in World.Filter<Spaceship>() )
        {
            if ( !World.Has<Mesh>( entity ) )
            {
                Mesh mesh = CreateSpaceshipMesh( _gl );
                World.Add( entity, mesh );
            }
        }
    }

    protected override void OnDestroy() { }

    private void HandleInput( ref Spaceship spaceship, ref Transform transform, SpaceshipConfig config,
        float deltaTime )
    {
        if ( _inputSystem == null || _input == null ) return;

        float angle = GetAngleFromQuaternion( transform.Rotation );

        if ( _inputSystem.IsKeyDown( _input, Key.A ) || _inputSystem.IsKeyDown( _input, Key.Left ) )
        {
            spaceship.AngularVelocity += config.RotationSpeed * deltaTime;
        }

        if ( _inputSystem.IsKeyDown( _input, Key.D ) || _inputSystem.IsKeyDown( _input, Key.Right ) )
        {
            spaceship.AngularVelocity -= config.RotationSpeed * deltaTime;
        }

        if ( _inputSystem.IsKeyDown( _input, Key.W ) || _inputSystem.IsKeyDown( _input, Key.Up ) )
        {
            Vector2D<float> direction = new(
                MathF.Cos( angle ),
                MathF.Sin( angle )
            );

            spaceship.Velocity += direction * config.Acceleration * deltaTime;

            float speed = MathF.Sqrt( spaceship.Velocity.X * spaceship.Velocity.X +
                                      spaceship.Velocity.Y * spaceship.Velocity.Y );
            if ( speed > config.MaxSpeed )
            {
                spaceship.Velocity = spaceship.Velocity / speed * config.MaxSpeed;
            }

            ParticleSystem? particleSystem = World.GetSystem<ParticleSystem>();
            if ( particleSystem != null )
            {
                Vector3D<float> enginePos = transform.Position;
                enginePos.X -= MathF.Cos( angle ) * 0.4f;
                enginePos.Y -= MathF.Sin( angle ) * 0.4f;

                particleSystem.SpawnEngineParticles( enginePos, angle, 2 );
            }
        }

        if ( _inputSystem.IsKeyDown( _input, Key.Space ) && spaceship.ShootCooldown <= 0 )
        {
            BulletSystem? bulletSystem = World.GetSystem<BulletSystem>();
            if ( bulletSystem != null )
            {
                Vector2D<float> direction = new(
                    MathF.Cos( angle ),
                    MathF.Sin( angle )
                );

                bulletSystem.SpawnBullet( transform.Position, direction );
                spaceship.ShootCooldown = config.ShootCooldown;
            }
        }
    }

    private static float GetAngleFromQuaternion( Quaternion<float> rotation )
    {
        return 2f * MathF.Atan2( rotation.Z, rotation.W );
    }

    // Оборачиваем позицию корабля при выходе за границы экрана
    private void WrapPosition( ref Vector3D<float> position, WorldBounds bounds )
    {
        float halfWidth = bounds.Width / 2;
        float halfHeight = bounds.Height / 2;

        if ( position.X > halfWidth ) position.X = -halfWidth;
        if ( position.X < -halfWidth ) position.X = halfWidth;
        if ( position.Y > halfHeight ) position.Y = -halfHeight;
        if ( position.Y < -halfHeight ) position.Y = halfHeight;
    }

    private SpaceshipConfig GetConfig()
    {
        foreach ( Entity entity in World.Filter<SpaceshipConfig>() )
        {
            return World.Get<SpaceshipConfig>( entity );
        }

        return SpaceshipConfig.Default;
    }

    private WorldBounds GetWorldBounds()
    {
        foreach ( Entity entity in World.Filter<WorldBounds>() )
        {
            return World.Get<WorldBounds>( entity );
        }

        return WorldBounds.Default;
    }

    private static Mesh CreateSpaceshipMesh( GL gl )
    {
        Vertex[] vertices = CreateSpaceshipVertices();
        uint[] indices = CreateSpaceshipIndices();

        Mesh mesh = MeshSystem.CreateMeshFromVertices( gl, vertices, indices );
        mesh.Topology = PrimitiveType.LineStrip;
        mesh.Material = CreateWhiteMaterial();

        return mesh;
    }

    private static Vertex[] CreateSpaceshipVertices()
    {
        return
        [
            new Vertex( new Vector3D<float>( 0.5f, 0f, 0f ), Vector2D<float>.Zero, new Vector4D<float>( 1, 1, 1, 1 ), 0,
                Vector3D<float>.UnitZ ),
            new Vertex( new Vector3D<float>( -0.3f, 0.3f, 0f ), Vector2D<float>.Zero, new Vector4D<float>( 1, 1, 1, 1 ), 0,
                Vector3D<float>.UnitZ ),
            new Vertex( new Vector3D<float>( -0.2f, 0f, 0f ), Vector2D<float>.Zero, new Vector4D<float>( 1, 1, 1, 1 ), 0,
                Vector3D<float>.UnitZ ),
            new Vertex( new Vector3D<float>( -0.3f, -0.3f, 0f ), Vector2D<float>.Zero, new Vector4D<float>( 1, 1, 1, 1 ), 0,
                Vector3D<float>.UnitZ )
        ];
    }

    private static uint[] CreateSpaceshipIndices()
    {
        return [ 0, 1, 2, 3, 0 ];
    }

    private static Material CreateWhiteMaterial()
    {
        return new Material
        {
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f )
        };
    }
}