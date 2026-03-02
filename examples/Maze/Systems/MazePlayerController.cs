using Silk.NET.Input;
using Silk.NET.Maths;
using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using Maze.Components;

namespace Maze.Systems;

public class MazePlayerController : SystemBase
{
    private InputSystem? _inputSystem;
    private Vector2D<float> _lastMousePos;
    private bool _firstMouse = true;
    private bool _cursorVisible;
    private float _yaw = -90f;
    private float _pitch;
    private const float _playerHeight = 0.5f;
    private const float _playerRadius = 0.3f;
    private const float _movementSpeed = 3f;
    private const float _mouseSensitivity = 0.1f;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();

        if ( World.HasGlobal<Input>() )
        {
            Input input = World.GetGlobal<Input>();
            input.Mouse.Cursor.CursorMode = CursorMode.Disabled;
        }
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( !World.HasGlobal<Input>() || _inputSystem == null )
            return;

        Input input = World.GetGlobal<Input>();
        MazeSystem? mazeSystem = World.GetSystem<MazeSystem>();

        foreach ( Entity entity in World.Filter<Camera, Transform>() )
        {
            ref Transform transform = ref World.Get<Transform>( entity );

            HandleCursorToggle( input );

            if ( !_cursorVisible )
            {
                HandleMovement( ref transform, input, mazeSystem, deltaTime );
                HandleMouseLook( ref transform, input );
            }
        }
    }

    private void HandleCursorToggle( Input input )
    {
        bool shiftPressed = _inputSystem!.IsKeyDown( input, Key.ShiftLeft ) ||
                           _inputSystem.IsKeyDown( input, Key.ShiftRight );

        if ( shiftPressed != _cursorVisible )
        {
            _cursorVisible = shiftPressed;
            input.Mouse.Cursor.CursorMode = _cursorVisible ? CursorMode.Normal : CursorMode.Disabled;
            _firstMouse = true;
        }
    }

    private void HandleMovement( ref Transform transform, Input input, MazeSystem? mazeSystem, float deltaTime )
    {
        Vector3D<float> originalPos = transform.Position;
        Vector3D<float> moveDirection = CalculateMoveDirection( input );
        Vector3D<float> desiredPos = CalculateDesiredPosition( originalPos, moveDirection, deltaTime );

        transform.Position = mazeSystem != null
            ? ResolveCollision( originalPos, desiredPos, mazeSystem )
            : desiredPos;
    }

    private Vector3D<float> CalculateMoveDirection( Input input )
    {
        Vector3D<float> moveDir = Vector3D<float>.Zero;

        if ( _inputSystem!.IsKeyDown( input, Key.W ) )
            moveDir += CameraSystem.GetForwardFlat( _yaw );
        if ( _inputSystem.IsKeyDown( input, Key.S ) )
            moveDir -= CameraSystem.GetForwardFlat( _yaw );
        if ( _inputSystem.IsKeyDown( input, Key.A ) )
            moveDir -= CameraSystem.GetRightFlat( _yaw );
        if ( _inputSystem.IsKeyDown( input, Key.D ) )
            moveDir += CameraSystem.GetRightFlat( _yaw );

        return moveDir;
    }

    private Vector3D<float> CalculateDesiredPosition( Vector3D<float> originalPos, Vector3D<float> moveDir, float deltaTime )
    {
        Vector3D<float> desiredPos = originalPos;

        if ( moveDir != Vector3D<float>.Zero )
        {
            moveDir = Vector3D.Normalize( moveDir );
            Vector3D<float> velocity = moveDir * _movementSpeed * deltaTime;
            desiredPos += velocity;
        }

        return new Vector3D<float>( desiredPos.X, _playerHeight, desiredPos.Z );
    }

    private void HandleMouseLook( ref Transform transform, Input input )
    {
        Vector2D<float> mouseDelta = GetMouseDelta( input );
        UpdateCameraRotation( mouseDelta );
        CameraSystem.SetLookDirection( ref transform, _yaw, _pitch );
    }

    private Vector2D<float> GetMouseDelta( Input input )
    {
        System.Numerics.Vector2 mousePos = _inputSystem!.GetMousePosition( input );
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

    private void UpdateCameraRotation( Vector2D<float> mouseDelta )
    {
        _yaw += mouseDelta.X * _mouseSensitivity;
        _pitch += mouseDelta.Y * _mouseSensitivity;
        _pitch = Math.Clamp( _pitch, -45f, 45f );
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }

    private Vector3D<float> ResolveCollision( Vector3D<float> from, Vector3D<float> to, MazeSystem mazeSystem )
    {
        if ( !mazeSystem.CheckCollision( to.X, to.Z, _playerRadius ) )
            return to;

        Vector3D<float> xOnly = new( to.X, from.Y, from.Z );
        if ( !mazeSystem.CheckCollision( xOnly.X, xOnly.Z, _playerRadius ) )
            return xOnly;

        Vector3D<float> zOnly = new( from.X, from.Y, to.Z );
        if ( !mazeSystem.CheckCollision( zOnly.X, zOnly.Z, _playerRadius ) )
            return zOnly;

        return from;
    }
}
