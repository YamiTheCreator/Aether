using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Graphics;

public sealed class WindowBase : IDisposable
{
    private static IWindow? _window;

    public event Action? OnLoad;
    public event Action<double>? OnUpdate;
    public event Action<double>? OnRender;
    public event Action? OnResize;
    public event Action? OnClosing;

    public static GL Gl { get; private set; } = null!;

    public static IInputContext Input { get; private set; } = null!;

    public static int Width => _window!.FramebufferSize.X;
    public static int Height => _window!.FramebufferSize.Y;

    public static int LogicalWidth => _window!.Size.X;
    public static int LogicalHeight => _window!.Size.Y;

    public static void SetResizable( bool resizable )
    {
        _window?.WindowBorder = resizable ? WindowBorder.Resizable : WindowBorder.Fixed;
    }

    public static void SetFullScreen( bool fullscreen )
    {
        _window?.WindowState = fullscreen ? WindowState.Maximized : WindowState.Normal;
    }

    public WindowBase( WindowOptions? opts )
    {
        WindowOptions options = opts ?? WindowOptions.Default;

        options.API = new GraphicsAPI(
            ContextAPI.OpenGL,
            ContextProfile.Core,
            ContextFlags.Default,
            new APIVersion( 3, 3 )
        );

        options.VSync = true;
        options.ShouldSwapAutomatically = true;
        options.IsVisible = true;

        _window = Window.Create( options );

        _window.Load += Load;
        _window.Update += Update;
        _window.Render += Render;
        _window.Resize += Resize;
        _window.Closing += Closing;
    }

    public void Run()
    {
        _window?.Run();
    }

    private void Load()
    {
        Gl = _window.CreateOpenGL();
        Gl.Viewport( 0, 0, ( uint )_window?.FramebufferSize.X!, ( uint )_window.FramebufferSize.Y );
        Input = _window.CreateInput();

        Gl.Enable( EnableCap.Blend );
        Gl.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
        Gl.Enable( EnableCap.DepthTest );

        OnLoad?.Invoke();
    }

    private void Update( double dt ) => OnUpdate?.Invoke( dt );
    private void Render( double dt ) => OnRender?.Invoke( dt );

    private void Resize( Vector2D<int> size )
    {
        Gl.Viewport( 0, 0, ( uint )_window?.FramebufferSize.X!, ( uint )_window.FramebufferSize.Y );
        OnResize?.Invoke();
    }

    private void Closing() => OnClosing?.Invoke();

    public void Close() => _window?.Close();

    public void Dispose()
    {
        Input.Dispose();
        Gl.Dispose();
        _window?.Dispose();
    }
}