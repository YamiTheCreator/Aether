using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace SphereToTor.Systems;

public class OrbitCameraController : SystemBase
{
    private InputSystem? _inputSystem;
    private Input? _input;
    private Vector2D<float> _lastMousePos;
    private bool _firstMouse = true;
    private float _yaw = 45f;
    private float _pitch = 20f;
    private float _distance = 4f;
    private const float _mouseSensitivity = 0.2f;
    private const float _minDistance = 2f;
    private const float _maxDistance = 10f;
    private const float _scrollSpeed = 0.5f;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();
    }
    
    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem is null || _input is null )
            return;

        foreach ( Entity entity in World.Filter<Camera, Transform>() )
        {
            ref Transform transform = ref World.Get<Transform>( entity );
            
            HandleMouseRotation();
            HandleZoom( deltaTime );
            UpdateCameraPosition( ref transform );
        }
    }
    
    private void HandleMouseRotation()
    {
        System.Numerics.Vector2 mousePos = _inputSystem!.GetMousePosition( _input! );
        Vector2D<float> mousePosVec = new( mousePos.X, mousePos.Y );

        if ( _firstMouse )
        {
            _lastMousePos = mousePosVec;
            _firstMouse = false;
            return;
        }
        
        if ( _inputSystem.IsMouseButtonDown( _input, MouseButton.Left ) )
        {
            Vector2D<float> delta = new(
                mousePosVec.X - _lastMousePos.X,
                _lastMousePos.Y - mousePosVec.Y
            );

            _yaw += delta.X * _mouseSensitivity;
            _pitch += delta.Y * _mouseSensitivity;
            _pitch = Math.Clamp( _pitch, -89f, 89f );
        }

        _lastMousePos = mousePosVec;
    }
    
    private void HandleZoom( float deltaTime )
    {
        float zoomDelta = 0f;

        if ( _inputSystem!.IsKeyDown( _input!, Key.Q ) )
            zoomDelta -= _scrollSpeed * deltaTime * 10f;
        if ( _inputSystem.IsKeyDown( _input!, Key.E ) )
            zoomDelta += _scrollSpeed * deltaTime * 10f;

        if ( zoomDelta != 0f )
        {
            _distance += zoomDelta;
            _distance = Math.Clamp( _distance, _minDistance, _maxDistance );
        }
    }
    
    private void UpdateCameraPosition( ref Transform transform )
    {
        CameraSystem.SetOrbitPosition(
            ref transform,
            target: Vector3D<float>.Zero,
            distance: _distance,
            yaw: _yaw,
            pitch: _pitch
        );
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }
}
