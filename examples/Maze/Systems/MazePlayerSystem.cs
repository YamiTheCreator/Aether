using Silk.NET.Input;
using Silk.NET.Maths;
using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using Maze.Components;

namespace Maze.Systems;

public class MazePlayerSystem : SystemBase
{
    private readonly CameraSystem _cameraSystem = new();
    private readonly InputSystem _inputSystem = new();
    private Vector2D<float> _lastMousePos;
    private bool _firstMouse = true;
    private bool _cursorVisible;
    private const float _playerHeight = 0.5f;
    private const float _playerRadius = 0.3f;

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

        MazeGrid? mazeGrid = null;
        foreach ( Entity gridEntity in World.Filter<MazeGrid>() )
        {
            mazeGrid = World.Get<MazeGrid>( gridEntity );
            break;
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
                Vector3D<float> originalPos = transform.Position;
                Vector3D<float> desiredPos = originalPos;

                Vector3D<float> moveDir = Vector3D<float>.Zero;

                if ( _inputSystem.IsKeyDown( input, Key.W ) )
                {
                    Vector3D<float> forward = new(
                        MathF.Cos( camera.Yaw * MathF.PI / 180f ),
                        0,
                        MathF.Sin( camera.Yaw * MathF.PI / 180f )
                    );
                    moveDir += Vector3D.Normalize( forward );
                }

                if ( _inputSystem.IsKeyDown( input, Key.S ) )
                {
                    Vector3D<float> forward = new(
                        MathF.Cos( camera.Yaw * MathF.PI / 180f ),
                        0,
                        MathF.Sin( camera.Yaw * MathF.PI / 180f )
                    );
                    moveDir -= Vector3D.Normalize( forward );
                }

                if ( _inputSystem.IsKeyDown( input, Key.A ) )
                {
                    Vector3D<float> right = new(
                        MathF.Cos( ( camera.Yaw + 90f ) * MathF.PI / 180f ),
                        0,
                        MathF.Sin( ( camera.Yaw + 90f ) * MathF.PI / 180f )
                    );
                    moveDir -= Vector3D.Normalize( right );
                }

                if ( _inputSystem.IsKeyDown( input, Key.D ) )
                {
                    Vector3D<float> right = new(
                        MathF.Cos( ( camera.Yaw + 90f ) * MathF.PI / 180f ),
                        0,
                        MathF.Sin( ( camera.Yaw + 90f ) * MathF.PI / 180f )
                    );
                    moveDir += Vector3D.Normalize( right );
                }

                if ( moveDir != Vector3D<float>.Zero )
                {
                    moveDir = Vector3D.Normalize( moveDir );
                    Vector3D<float> velocity = moveDir * camera.MovementSpeed * deltaTime;
                    desiredPos += velocity;
                }

                desiredPos = new Vector3D<float>( desiredPos.X, _playerHeight, desiredPos.Z );

                if ( mazeGrid.HasValue )
                {
                    Vector3D<float> finalPos = ResolveCollision( originalPos, desiredPos, mazeGrid.Value );
                    transform.Position = finalPos;
                }
                else
                {
                    transform.Position = desiredPos;
                }

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

                camera.Pitch = Math.Clamp( camera.Pitch, -45f, 45f );
            }
        }
    }

    private Vector3D<float> ResolveCollision( Vector3D<float> from, Vector3D<float> to, MazeGrid grid )
    {
        if ( !MazeCollisionHelper.CheckCollision( grid, to.X, to.Z, _playerRadius ) )
            return to;

        Vector3D<float> xOnly = new( to.X, from.Y, from.Z );
        if ( !MazeCollisionHelper.CheckCollision( grid, xOnly.X, xOnly.Z, _playerRadius ) )
            return xOnly;

        Vector3D<float> zOnly = new( from.X, from.Y, to.Z );
        if ( !MazeCollisionHelper.CheckCollision( grid, zOnly.X, zOnly.Z, _playerRadius ) )
            return zOnly;

        return from;
    }
}