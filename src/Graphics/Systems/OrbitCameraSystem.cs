using Aether.Core;
using Silk.NET.Maths;
using Silk.NET.Input;
using Graphics.Components;

namespace Graphics.Systems;

public class OrbitCameraSystem : SystemBase
{
    private readonly InputSystem _inputSystem = new();
    private Vector2D<float> _lastMousePos;
    private bool _isRotating;

    protected override void OnUpdate( float deltaTime )
    {
        if ( !World.HasGlobal<Input>() )
            return;

        Input input = World.GetGlobal<Input>();

        foreach ( Entity entity in World.Filter<Camera>() )
        {
            ref Camera camera = ref World.Get<Camera>( entity );

            if ( !camera.IsOrbitMode )
                continue;

            HandleMouseInput( ref camera, input );

            HandleKeyboardZoom( ref camera, input, deltaTime );

            UpdateCameraPosition( ref camera );
        }
    }

    private void HandleMouseInput( ref Camera camera, Input input )
    {
        System.Numerics.Vector2 mousePosNum = _inputSystem.GetMousePosition( input );
        Vector2D<float> mousePos = new( mousePosNum.X, mousePosNum.Y );

        bool isRightButtonDown = _inputSystem.IsMouseButtonDown( input, MouseButton.Right );

        if ( isRightButtonDown )
        {
            if ( !_isRotating )
            {
                _isRotating = true;
                _lastMousePos = mousePos;
            }
            else
            {
                Vector2D<float> delta = mousePos - _lastMousePos;

                camera.OrbitYaw += delta.X * camera.OrbitRotationSpeed;
                camera.OrbitPitch -= delta.Y * camera.OrbitRotationSpeed;

                camera.OrbitPitch = Math.Clamp( camera.OrbitPitch, -89f, 89f );

                _lastMousePos = mousePos;
            }
        }
        else
        {
            _isRotating = false;
        }

        float scroll = input.Mouse.ScrollWheels[ 0 ].Y;
        if ( scroll != 0 )
        {
            camera.OrbitDistance -= scroll * camera.OrbitZoomSpeed;
            camera.OrbitDistance = Math.Clamp(
                camera.OrbitDistance,
                camera.OrbitMinDistance,
                camera.OrbitMaxDistance );
        }
    }

    private void HandleKeyboardZoom( ref Camera camera, Input input, float deltaTime )
    {
        float zoomDelta = 0f;

        if ( _inputSystem.IsKeyDown( input, Key.Equal ) || _inputSystem.IsKeyDown( input, Key.KeypadAdd ) )
        {
            zoomDelta = -2f * deltaTime;
        }
        else if ( _inputSystem.IsKeyDown( input, Key.Minus ) || _inputSystem.IsKeyDown( input, Key.KeypadSubtract ) )
        {
            zoomDelta = 2f * deltaTime;
        }

        if ( zoomDelta != 0 )
        {
            camera.OrbitDistance += zoomDelta;
            camera.OrbitDistance = Math.Clamp(
                camera.OrbitDistance,
                camera.OrbitMinDistance,
                camera.OrbitMaxDistance );
        }
    }

    private void UpdateCameraPosition( ref Camera camera )
    {
        float yawRad = camera.OrbitYaw * ( MathF.PI / 180f );
        float pitchRad = camera.OrbitPitch * ( MathF.PI / 180f );

        Vector3D<float> position;
        position.X = camera.OrbitTarget.X + camera.OrbitDistance * MathF.Cos( pitchRad ) * MathF.Sin( yawRad );
        position.Y = camera.OrbitTarget.Y + camera.OrbitDistance * MathF.Sin( pitchRad );
        position.Z = camera.OrbitTarget.Z + camera.OrbitDistance * MathF.Cos( pitchRad ) * MathF.Cos( yawRad );

        camera.StaticPosition = position;

        Vector3D<float> direction = Vector3D.Normalize( camera.OrbitTarget - position );

        camera.Forward = direction;
        camera.Right = Vector3D.Normalize( Vector3D.Cross( direction, Vector3D<float>.UnitY ) );
        camera.Up = Vector3D.Normalize( Vector3D.Cross( camera.Right, direction ) );
    }
}