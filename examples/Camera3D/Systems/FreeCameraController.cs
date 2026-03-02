using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Camera3D.Systems;

public class FreeCameraController : SystemBase
{
    private InputSystem? _inputSystem;
    private Input? _input;
    private Vector2D<float> _lastMousePos;
    private bool _firstMouse = true;
    private float _yaw = -90f;
    private float _pitch;
    private const float _movementSpeed = 5f;
    private const float _mouseSensitivity = 0.1f;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();

        if ( _input != null )
        {
            _input.Mouse.Cursor.CursorMode = CursorMode.Disabled;
        }
    }
    
    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem is null || _input is null )
            return;

        foreach ( Entity entity in World.Filter<Camera, Transform>() )
        {
            ref Transform transform = ref World.Get<Transform>( entity );

            HandleMovement( ref transform, deltaTime );
            HandleMouseLook( ref transform );
        }
    }
    
    private void HandleMovement( ref Transform transform, float deltaTime )
    {
        Vector3D<float> moveDir = CalculateMoveDirection( transform );

        if ( moveDir != Vector3D<float>.Zero )
        {
            moveDir = Vector3D.Normalize( moveDir );
            transform.Position += moveDir * _movementSpeed * deltaTime;
        }
    }
    
    private Vector3D<float> CalculateMoveDirection( Transform transform )
    {
        Vector3D<float> moveDir = Vector3D<float>.Zero;

        if ( _inputSystem!.IsKeyDown( _input!, Key.W ) )
            moveDir += transform.Forward;
        if ( _inputSystem.IsKeyDown( _input!, Key.S ) )
            moveDir -= transform.Forward;
        if ( _inputSystem.IsKeyDown( _input!, Key.A ) )
            moveDir -= transform.Right;
        if ( _inputSystem.IsKeyDown( _input!, Key.D ) )
            moveDir += transform.Right;
        if ( _inputSystem.IsKeyDown( _input!, Key.Space ) )
            moveDir += Vector3D<float>.UnitY;
        if ( _inputSystem.IsKeyDown( _input!, Key.ShiftLeft ) )
            moveDir -= Vector3D<float>.UnitY;

        return moveDir;
    }
    
    private void HandleMouseLook( ref Transform transform )
    {
        Vector2D<float> mouseDelta = GetMouseDelta();
        UpdateCameraRotation( mouseDelta );
        CameraSystem.SetLookDirection( ref transform, _yaw, _pitch );
    }
    
    private Vector2D<float> GetMouseDelta()
    {
        System.Numerics.Vector2 mousePos = _inputSystem!.GetMousePosition( _input! );
        Vector2D<float> mousePosVec = new( mousePos.X, mousePos.Y );

        if ( _firstMouse )
        {
            _lastMousePos = mousePosVec;
            _firstMouse = false;
            return Vector2D<float>.Zero;
        }

        Vector2D<float> delta = new(
            mousePosVec.X - _lastMousePos.X,
            _lastMousePos.Y - mousePosVec.Y
        );

        _lastMousePos = mousePosVec;
        return delta;
    }
    
    // Pitch ограничен диапазоном [-89°, 89°] чтобы избежать переворота камеры
    private void UpdateCameraRotation( Vector2D<float> mouseDelta )
    {
        _yaw += mouseDelta.X * _mouseSensitivity;
        _pitch += mouseDelta.Y * _mouseSensitivity;
        _pitch = Math.Clamp( _pitch, -89f, 89f );
    }

    protected override void OnRender() { }

    protected override void OnDestroy() { }
}
