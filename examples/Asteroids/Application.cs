using Aether.Core;
using Asteroids.Components;
using Asteroids.Systems;
using Graphics;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Asteroids;

public class Application
{
    private WindowBase? _window;
    private World? _world;

    public void Run()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>( 1280, 720 );
        options.Title = "Asteroids";

        _window = new WindowBase( options );

        _window.OnLoad += OnLoad;
        _window.OnUpdate += OnUpdate;
        _window.OnRender += OnRender;
        _window.OnClosing += OnClosing;

        _window.Run();
    }

    private void OnLoad()
    {
        _world = new World();

        InputSystem inputSystem = new();
        Input input = inputSystem.CreateInput( WindowBase.Input );

        TextureObject whiteTextureObj = TextureObject.FromColor( WindowBase.Gl, 1, 1 );
        Texture2D whiteTexture = new()
        {
            Texture = whiteTextureObj
        };

        _world.AddSystem( new ShaderSystem( WindowBase.Gl ) );
        _world.AddSystem( new MaterialSystem() );
        _world.AddSystem( inputSystem );
        _world.AddSystem( new CameraSystem() );
        _world.AddSystem( new SpriteSystem( WindowBase.Gl ) );
        _world.AddSystem( new MeshSystem( WindowBase.Gl ) );

        _world.AddSystem( new GameStateSystem() );
        _world.AddSystem( new SpaceshipSystem() );
        _world.AddSystem( new AsteroidSystem() );
        _world.AddSystem( new BulletSystem() );
        _world.AddSystem( new ParticleSystem() );
        _world.AddSystem( new AsteroidsCollisionSystem() );

        _world.SetGlobal( inputSystem );
        _world.SetGlobal( input );
        _world.SetGlobal( whiteTexture );
        _world.SetGlobal( WindowBase.Gl );

        _world.Init();

        _world.Spawn( SpaceshipConfig.Default );
        _world.Spawn( WorldBounds.Default );
        _world.Spawn( BulletConfig.Default );

        CameraSystem.CreateOrthographicCamera(
            _world,
            position: new Vector3D<float>( 0, 0, 0 )
        );

        AsteroidSystem? asteroidSystem = _world.GetSystem<AsteroidSystem>();
        if ( asteroidSystem != null )
        {
            for ( int i = 0; i < 5; i++ )
            {
                asteroidSystem.SpawnAsteroid( 3 );
            }
        }
    }

    private void OnUpdate( double dt )
    {
        _world?.Update( ( float )dt );
        UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
        if ( _world == null || _window == null ) return;

        GameStateSystem? gameStateSystem = _world.GetSystem<GameStateSystem>();
        if ( gameStateSystem == null ) return;

        int score = gameStateSystem.GetScore();
        int lives = gameStateSystem.GetLives();
        int wave = gameStateSystem.GetWaveNumber();
        bool isGameOver = gameStateSystem.IsGameOver();

        if ( isGameOver )
        {
            _window.SetTitle( $"Asteroids - GAME OVER! Final Score: {score} - Press R to Restart" );
        }
        else
        {
            _window.SetTitle( $"Asteroids - Wave {wave} | Score: {score} | Lives: {lives}" );
        }
    }

    private void OnRender( double dt )
    {
        WindowBase.Gl.ClearColor( 0.05f, 0.05f, 0.1f, 1f );
        _world?.Render();
    }

    private void OnClosing()
    {
        _world?.Dispose();
    }
}