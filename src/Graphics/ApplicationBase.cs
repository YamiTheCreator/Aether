using Silk.NET.Maths;
using Aether.Core;
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
    bool fullScreen = false )
{
    private WindowBase WindowBase { get; set; } = null!;
    protected World World { get; private set; } = null!;
    private InputSystem _inputSystem = null!;
    private Input _input = null!;

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

        OnInitialize();

        World.Init();
    }

    protected abstract void OnInitialize();

    private void OnUpdate( double deltaTime )
    {
        _inputSystem.Update( _input );

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
        World.Dispose();
    }
}