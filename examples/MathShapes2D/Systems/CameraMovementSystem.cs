using Aether.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;

namespace MathShapes2D.Systems;

public class CameraMovementSystem : SystemBase
{
    private InputSystem? _inputSystem;
    private Input? _input;
    private int _currentPlane;
    private const int _totalPlanes = 3;
    private const float _planeSpacing = 8f;
    private bool _wasAPressed;
    private bool _wasDPressed;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem is null || _input is null )
            return;

        HandlePlaneSwitch();
    }

    private void HandlePlaneSwitch()
    {
        bool isAPressed = _inputSystem!.IsKeyDown( _input!, Key.A );
        bool isDPressed = _inputSystem.IsKeyDown( _input, Key.D );

        if ( isAPressed && !_wasAPressed && _currentPlane > 0 )
        {
            _currentPlane--;
            UpdateCameraPosition();
        }
        else if ( isDPressed && !_wasDPressed && _currentPlane < _totalPlanes - 1 )
        {
            _currentPlane++;
            UpdateCameraPosition();
        }

        _wasAPressed = isAPressed;
        _wasDPressed = isDPressed;
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }

    private void UpdateCameraPosition()
    {
        foreach ( Entity entity in World.Filter<Camera, Transform>() )
        {
            ref Transform transform = ref World.Get<Transform>( entity );
            transform.Position = new Vector3D<float>( _currentPlane * _planeSpacing, 0f, 5f );
        }
    }
}