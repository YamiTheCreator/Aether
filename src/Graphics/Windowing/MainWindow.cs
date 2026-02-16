using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Graphics.Windowing;

/// <summary>
/// MainWindow wrapper for Silk.NET windowing with OpenGL context.
/// </summary>
public sealed class MainWindow : IDisposable
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

    public static void SetResizable( bool resizable )
    {
        _window?.WindowBorder = resizable ? WindowBorder.Resizable : WindowBorder.Fixed;
    }

    public MainWindow( WindowOptions? opts )
    {
        WindowOptions options = opts ?? WindowOptions.Default;

        // Configure OpenGL context for macOS compatibility
        options.API = new GraphicsAPI(
            ContextAPI.OpenGL,
            ContextProfile.Core,
            ContextFlags.Default, // Changed from ForwardCompatible
            new APIVersion( 3, 3 ) // Changed from 4.1 to 3.3 for better compatibility
        );

        options.VSync = true;
        options.ShouldSwapAutomatically = true;
        options.IsVisible = true; // Explicitly set visible

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

        global::Graphics.Input.Input.Init( Input );

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