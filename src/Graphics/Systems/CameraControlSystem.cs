using Silk.NET.Input;
using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics.Components;

namespace Graphics.Systems;

public class CameraControlSystem : SystemBase
{
    private readonly CameraSystem _cameraSystem = new();
    private readonly InputSystem _inputSystem = new();
    private Vector2D<float> _lastMousePos;
    private bool _firstMouse = true;
    private bool _cursorVisible;

    protected override void OnInit()
    {
        if ( World.HasGlobal<Input>() )
        {
            Input input = World.GetGlobal<Input>();
            input.Mouse.Cursor.CursorMode = CursorMode.Disabled;
        }
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( !World.HasGlobal<Input>() )
            return;

        Input input = World.GetGlobal<Input>();

        if ( World.HasGlobal<InputSystem>() )
        {
            InputSystem inputSystem = World.GetGlobal<InputSystem>();
            inputSystem.Update( ref input );
            World.SetGlobal( input );
        }

        foreach ( Entity entity in World.Filter<Camera>().With<Transform>() )
        {
            ref Camera camera = ref World.Get<Camera>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            if ( camera.IsStatic )
                continue;

            bool shiftPressed = _inputSystem.IsKeyDown( input, Key.ShiftLeft ) ||
                                _inputSystem.IsKeyDown( input, Key.ShiftRight );

            if ( shiftPressed != _cursorVisible )
            {
                _cursorVisible = shiftPressed;
                input.Mouse.Cursor.CursorMode = _cursorVisible ? CursorMode.Normal : CursorMode.Disabled;
                _firstMouse = true;
            }

            if ( !_cursorVisible )
            {
                if ( _inputSystem.IsKeyDown( input, Key.W ) )
                    _cameraSystem.ProcessKeyboard( ref camera, ref transform, MovementType.Forward, deltaTime );
                if ( _inputSystem.IsKeyDown( input, Key.S ) )
                    _cameraSystem.ProcessKeyboard( ref camera, ref transform, MovementType.Backward, deltaTime );
                if ( _inputSystem.IsKeyDown( input, Key.A ) )
                    _cameraSystem.ProcessKeyboard( ref camera, ref transform, MovementType.Left, deltaTime );
                if ( _inputSystem.IsKeyDown( input, Key.D ) )
                    _cameraSystem.ProcessKeyboard( ref camera, ref transform, MovementType.Right, deltaTime );
                if ( _inputSystem.IsKeyDown( input, Key.Space ) )
                    _cameraSystem.ProcessKeyboard( ref camera, ref transform, MovementType.Up, deltaTime );
                if ( _inputSystem.IsKeyDown( input, Key.ControlLeft ) ||
                     _inputSystem.IsKeyDown( input, Key.ControlRight ) )
                    _cameraSystem.ProcessKeyboard( ref camera, ref transform, MovementType.Down, deltaTime );

                System.Numerics.Vector2 mousePos = _inputSystem.GetMousePosition( input );
                Vector2D<float> mousePosVec = new( mousePos.X, mousePos.Y );

                if ( _firstMouse )
                {
                    _lastMousePos = mousePosVec;
                    _firstMouse = false;
                }

                float xOffset = mousePosVec.X - _lastMousePos.X;
                float yOffset = _lastMousePos.Y - mousePosVec.Y;

                _lastMousePos = mousePosVec;

                _cameraSystem.ProcessMouseMovement( ref camera, ref transform, -xOffset, yOffset );
            }
        }
    }
}