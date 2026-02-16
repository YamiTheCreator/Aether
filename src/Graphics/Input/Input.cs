using Silk.NET.Input;

namespace Graphics.Input;

public static class Input
{
    private static IKeyboard _keyboard = null!;
    private static IMouse _mouse = null!;
    private static readonly HashSet<Key> _previousKeys = [ ];
    private static readonly HashSet<Key> _currentKeys = [ ];

    internal static void Init( IInputContext context )
    {
        _keyboard = context.Keyboards[ 0 ];
        _mouse = context.Mice[ 0 ];
    }

    internal static void Update()
    {
        _previousKeys.Clear();
        foreach ( Key key in _currentKeys )
        {
            _previousKeys.Add( key );
        }

        _currentKeys.Clear();
        foreach ( Key key in Enum.GetValues<Key>() )
        {
            if ( _keyboard.IsKeyPressed( key ) )
            {
                _currentKeys.Add( key );
            }
        }
    }

    public static bool IsKeyDown( Key key ) => _keyboard.IsKeyPressed( key );

    public static bool IsKeyPressed( Key key ) => _currentKeys.Contains( key ) && !_previousKeys.Contains( key );

    public static bool IsKeyReleased( Key key ) => !_currentKeys.Contains( key ) && _previousKeys.Contains( key );

    public static bool IsMouseButtonDown( MouseButton button ) => _mouse.IsButtonPressed( button );

    public static System.Numerics.Vector2 MousePosition => _mouse.Position;
}