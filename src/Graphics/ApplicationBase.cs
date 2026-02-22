using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Aether.Core.Options;
using Graphics.Components;
using Graphics.Systems;
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
    private WindowBase WindowBase { get; set; } = null!;
    protected World World { get; private set; } = null!;
    private InputSystem _inputSystem = null!;
    private Components.Input _input;

    private readonly WindowOptions _windowOptions = new()
    {
        Title = title,
        Size = new Vector2D<int>( width, height ),
        VSync = true,
        WindowState = fullScreen ? WindowState.Maximized : WindowState.Normal,
    };

    private readonly BaseOptions _worldOptions = worldOptions ?? new BaseOptions();

    public void Run()
    {
        WindowBase = new WindowBase( _windowOptions );
        World = new World( _worldOptions );

        WindowBase.OnLoad += OnLoad;
        WindowBase.OnUpdate += OnUpdate;
        WindowBase.OnRender += OnRender;
        WindowBase.OnResize += OnResize;
        WindowBase.OnClosing += OnClosing;

        WindowBase.Run();
    }

    private void OnLoad()
    {
        World.SetGlobal( WindowBase );

        _inputSystem = new InputSystem();
        _input = _inputSystem.CreateInput( WindowBase.Input );
        World.SetGlobal( _input );
        World.SetGlobal( _inputSystem );

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
        _inputSystem.Update( ref _input );
        World.SetGlobal( _input );

        World.Update( ( float )deltaTime );

        if ( _inputSystem.IsKeyDown( _input, Key.Escape ) )
        {
            WindowBase.Close();
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
        RenderSystem? renderSystem = World.GetSystem<RenderSystem>();
        renderSystem?.Dispose();

        World.Dispose();
    }

    private Entity CreateDefaultCamera()
    {
        float aspectRatio = ( float )WindowBase.Width / WindowBase.Height;

        Entity cameraEntity = World.Spawn(
            new Transform( new Vector3D<float>( 0, 0, 0 ) ),
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

    protected Entity CreateStaticCamera( Vector3D<float> position, float size = 10f )
    {
        float aspectRatio = ( float )WindowBase.Width / WindowBase.Height;

        Entity cameraEntity = World.Spawn(
            new Transform( Vector3D<float>.Zero ),
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
        Vector3D<float> position,
        float fov = 45f,
        float nearPlane = 0.1f,
        float farPlane = 100f )
    {
        float aspectRatio = ( float )WindowBase.Width / WindowBase.Height;

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
        Vector3D<float> position,
        float size = 10f,
        float nearPlane = 0.1f,
        float farPlane = 100f )
    {
        float aspectRatio = ( float )WindowBase.Width / WindowBase.Height;

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