using Aether.Core;
using Silk.NET.Input;
using InputComponent = Graphics.Components.Input;

namespace Graphics.Systems;

public class InputSystem : SystemBase
{
    public InputComponent CreateInput( IInputContext context )
    {
        IKeyboard keyboard = context.Keyboards[ 0 ];
        IMouse mouse = context.Mice[ 0 ];

        return new InputComponent
        {
            Keyboard = keyboard,
            Mouse = mouse,
            PreviousKeys = [ ],
            CurrentKeys = [ ]
        };
    }

    private static readonly Key[] _allKeys = Enum.GetValues<Key>();

    protected override void OnCreate()
    {
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( World.HasGlobal<InputComponent>() )
        {
            InputComponent input = World.GetGlobal<InputComponent>();
            Update( input );
        }
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }

    public void Update( InputComponent input )
    {
        input.PreviousKeys.Clear();
        foreach ( Key key in input.CurrentKeys )
        {
            input.PreviousKeys.Add( key );
        }

        input.CurrentKeys.Clear();
        foreach ( Key key in _allKeys )
        {
            if ( key == Key.Unknown ) continue;
            try
            {
                if ( input.Keyboard.IsKeyPressed( key ) )
                {
                    input.CurrentKeys.Add( key );
                }
            }
            catch
            {
            }
        }
    }

    public bool IsKeyDown( InputComponent input, Key key ) => input.CurrentKeys.Contains( key );

    public bool IsKeyPressed( InputComponent input, Key key ) =>
        input.CurrentKeys.Contains( key ) && !input.PreviousKeys.Contains( key );

    public bool IsKeyReleased( InputComponent input, Key key ) =>
        !input.CurrentKeys.Contains( key ) && input.PreviousKeys.Contains( key );

    public bool IsMouseButtonDown( InputComponent input, MouseButton button ) =>
        input.Mouse.IsButtonPressed( button );

    public System.Numerics.Vector2 GetMousePosition( InputComponent input ) => input.Mouse.Position;
}
