using Aether.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using MathShapes2D.Components;

namespace MathShapes2D.Systems;

/// <summary>
/// Camera movement system - handles camera movement between planes
/// </summary>
public class CameraMovementSystem : SystemBase
{
    private readonly InputSystem _inputSystem = new();
    private int _currentPlane = 0;
    private const int TotalPlanes = 4;
    private const float PlaneSpacing = 8f;
    private bool _wasAPressed = false;
    private bool _wasDPressed = false;

    protected override void OnUpdate(float deltaTime)
    {
        if (!World.HasGlobal<Input>())
            return;

        Input input = World.GetGlobal<Input>();

        // Move camera left/right between planes with key press detection
        bool isAPressed = _inputSystem.IsKeyDown(input, Key.A);
        bool isDPressed = _inputSystem.IsKeyDown(input, Key.D);

        // Detect key press (not held)
        if (isAPressed && !_wasAPressed && _currentPlane > 0)
        {
            _currentPlane--;
            UpdateCameraPosition();
        }
        else if (isDPressed && !_wasDPressed && _currentPlane < TotalPlanes - 1)
        {
            _currentPlane++;
            UpdateCameraPosition();
        }

        _wasAPressed = isAPressed;
        _wasDPressed = isDPressed;
    }

    private void UpdateCameraPosition()
    {
        foreach (Entity entity in World.Filter<Camera>())
        {
            if (!World.Has<Transform>(entity))
                continue;

            ref Camera camera = ref World.Get<Camera>(entity);
            ref Transform transform = ref World.Get<Transform>(entity);

            float targetX = _currentPlane * PlaneSpacing;
            
            camera.StaticPosition = new Vector3D<float>(targetX, 0f, 5f);
            transform.Position = new Vector3D<float>(targetX, 0f, 5f);
        }
    }
}
