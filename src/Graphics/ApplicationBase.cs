using System.Numerics;
using Aether.Core;
using Aether.Core.Enums;
using Aether.Core.Options;
using Graphics.Components;
using Graphics.Windowing;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Graphics;

public abstract class ApplicationBase(
    string title,
    int width,
    int height,
    BaseOptions? worldOptions = null,
    bool fullScreen = false,
    bool createDefaultCamera = true )
{
    private MainWindow MainWindow { get; set; } = null!;
    protected World World { get; private set; } = null!;

    private readonly WindowOptions _windowOptions = new()
    {
        Title = title,
        Size = new Silk.NET.Maths.Vector2D<int>( width, height ),
        VSync = true,
        WindowState = fullScreen ? WindowState.Maximized : WindowState.Normal,
    };

    private readonly BaseOptions _worldOptions = worldOptions ?? new BaseOptions();

    public void Run()
    {
        MainWindow = new MainWindow( _windowOptions );
        World = new World( _worldOptions );

        MainWindow.OnLoad += OnLoad;
        MainWindow.OnUpdate += OnUpdate;
        MainWindow.OnRender += OnRender;
        MainWindow.OnResize += OnResize;
        MainWindow.OnClosing += OnClosing;

        MainWindow.Run();
    }

    private void OnLoad()
    {
        World.SetGlobal( MainWindow );

        if ( createDefaultCamera )
        {
            CreateDefaultCamera();
        }

        OnInitialize();

        World.Init();
    }

    protected abstract void OnInitialize();

    private void OnUpdate( double deltaTime )
    {
        Input.Input.Update();
        World.Update( ( float )deltaTime );

        if ( Input.Input.IsKeyDown( Key.Escape ) )
        {
            MainWindow.Close();
        }
    }

    private void OnRender( double deltaTime )
    {
        World.Render();
    }

    private void OnResize()
    {
    }

    private void OnClosing()
    {
        if ( World.HasGlobal<Renderer2D>() )
            World.GetGlobal<Renderer2D>().Dispose();

        World.Dispose();
    }

    private Entity CreateDefaultCamera()
    {
        float aspectRatio = ( float )MainWindow.Width / MainWindow.Height;

        Entity cameraEntity = World.Spawn(
            new Transform( new Vector3( 0, 0, 0 ) ),
            new Camera
            {
                ProjectionType = ProjectionType.Orthographic,
                AspectRatio = aspectRatio,
                OrthographicSize = 10f,
                NearPlane = 0.1f,
                FarPlane = 100f
            } );

        return cameraEntity;
    }

    protected Entity CreateStaticCamera( Vector3 position, float size = 10f )
    {
        float aspectRatio = ( float )MainWindow.Width / MainWindow.Height;

        Entity cameraEntity = World.Spawn(
            new Transform( Vector3.Zero ),
            new Camera
            {
                ProjectionType = ProjectionType.Orthographic,
                AspectRatio = aspectRatio,
                OrthographicSize = size,
                NearPlane = 0.1f,
                FarPlane = 100f,
                IsStatic = true,
                StaticPosition = position
            } );

        return cameraEntity;
    }

    protected Entity CreatePerspectiveCamera(
        Vector3 position,
        float fov = 45f,
        float nearPlane = 0.1f,
        float farPlane = 100f )
    {
        float aspectRatio = ( float )MainWindow.Width / MainWindow.Height;

        Entity cameraEntity = World.Spawn(
            new Transform( position ),
            new Camera
            {
                ProjectionType = ProjectionType.Perspective,
                FieldOfView = fov,
                AspectRatio = aspectRatio,
                NearPlane = nearPlane,
                FarPlane = farPlane
            } );

        return cameraEntity;
    }

    protected Entity CreateOrthographicCamera(
        Vector3 position,
        float size = 10f,
        float nearPlane = 0.1f,
        float farPlane = 100f )
    {
        float aspectRatio = ( float )MainWindow.Width / MainWindow.Height;

        Entity cameraEntity = World.Spawn(
            new Transform( position ),
            new Camera
            {
                ProjectionType = ProjectionType.Orthographic,
                OrthographicSize = size,
                AspectRatio = aspectRatio,
                NearPlane = nearPlane,
                FarPlane = farPlane
            } );

        return cameraEntity;
    }
}