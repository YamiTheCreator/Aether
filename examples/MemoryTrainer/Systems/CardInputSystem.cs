using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using MemoryTrainer.Components;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MemoryTrainer.Systems;

public class CardInputSystem : SystemBase
{
    private bool _wasMousePressed;
    private InputSystem? _inputSystem;
    private Input? _input;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();
    }
    
    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem is null || _input is null )
            return;

        GameState gameState = World.GetGlobal<GameState>();

        if ( HandleRestartInput() )
        {
            gameState.RestartRequested = true;
            World.SetGlobal( gameState );
            return;
        }

        if ( gameState.IsWaitingForFlipBack )
        {
            World.SetGlobal( gameState );
            return;
        }

        HandleMouseInput( ref gameState );
        World.SetGlobal( gameState );
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }
    
    private bool HandleRestartInput()
    {
        return _inputSystem!.IsKeyPressed( _input!, Key.R );
    }
    
    private void HandleMouseInput( ref GameState gameState )
    {
        bool isMousePressed = _inputSystem!.IsMouseButtonDown( _input!, MouseButton.Left );

        if ( isMousePressed && !_wasMousePressed )
        {
            ProcessMouseClick( ref gameState );
        }

        _wasMousePressed = isMousePressed;
    }

    // Преобразуем координаты экрана в луч и проверяем пересечение с картами
    private void ProcessMouseClick( ref GameState gameState )
    {
        System.Numerics.Vector2 mousePos = _inputSystem!.GetMousePosition( _input! );
        Vector2D<float> mousePosVec = new( mousePos.X, mousePos.Y );

        Entity? cameraEntity = GetCameraEntity();
        if ( !cameraEntity.HasValue )
            return;

        Camera camera = World.Get<Camera>( cameraEntity.Value );

        if ( UnprojectMouseToRay( mousePosVec, camera, out Vector3D<float> rayOrigin,
                out Vector3D<float> rayDirection ) )
        {
            CheckCardClick( rayOrigin, rayDirection, ref gameState );
        }
    }

    private Entity? GetCameraEntity()
    {
        foreach ( Entity entity in World.Filter<Camera>() )
        {
            return entity;
        }

        return null;
    }

    // Преобразуем координаты мыши в луч в мировом пространстве
    // Используем обратную матрицу view-projection для unprojection
    private bool UnprojectMouseToRay( Vector2D<float> mousePos, Camera camera,
        out Vector3D<float> rayOrigin, out Vector3D<float> rayDirection )
    {
        float width = WindowBase.LogicalWidth;
        float height = WindowBase.LogicalHeight;

        // Нормализовали координаты
        float x = 2f * mousePos.X / width - 1f;
        float y = 1f - 2f * mousePos.Y / height;

        Matrix4X4<float> viewMatrix = camera.ViewMatrix;
        Matrix4X4<float> projMatrix = camera.ProjectionMatrix;

        // Инвертировали матрицу для NDC to World
        Matrix4X4<float> viewProjMatrix = viewMatrix * projMatrix;
        if ( !Matrix4X4.Invert( viewProjMatrix, out Matrix4X4<float> invMatrix ) )
        {
            rayOrigin = Vector3D<float>.Zero;
            rayDirection = Vector3D<float>.Zero;
            return false;
        }

        // Взяли плоскости отсечения камеры
        Vector4D<float> rayClipNear = new( x, y, -1f, 1f );
        Vector4D<float> rayClipFar = new( x, y, 1f, 1f );

        Vector4D<float> rayWorldNear = Vector4D.Transform( rayClipNear, invMatrix );
        Vector4D<float> rayWorldFar = Vector4D.Transform( rayClipFar, invMatrix );

        // Отменяем перспективное искажение делением на w(трансляция)
        if ( MathF.Abs( rayWorldNear.W ) > 0.0001f )
            rayWorldNear /= rayWorldNear.W;
        if ( MathF.Abs( rayWorldFar.W ) > 0.0001f )
            rayWorldFar /= rayWorldFar.W;

        // Формируем луч между плоскостями в мировых координатах
        rayOrigin = new Vector3D<float>( rayWorldNear.X, rayWorldNear.Y, rayWorldNear.Z );
        Vector3D<float> rayEnd = new( rayWorldFar.X, rayWorldFar.Y, rayWorldFar.Z );
        rayDirection = Vector3D.Normalize( rayEnd - rayOrigin );

        return true;
    }

    // Проверяем пересечение луча с плоскостью карт и ищем карту в точке пересечения
    private void CheckCardClick( Vector3D<float> rayOrigin, Vector3D<float> rayDirection, ref GameState gameState )
    {
        Vector3D<float>? intersectionPoint = CalculatePlaneIntersection( rayOrigin, rayDirection );
        if ( !intersectionPoint.HasValue )
            return;

        Entity? clickedCard = FindCardAtPosition( intersectionPoint.Value );
        if ( clickedCard.HasValue )
        {
            gameState.ClickedCard = clickedCard.Value;
        }
    }

    // Вычисляем точку пересечения луча с горизонтальной плоскостью (Y = 0)
    private Vector3D<float>? CalculatePlaneIntersection( Vector3D<float> rayOrigin, Vector3D<float> rayDirection )
    {
        const float planeY = 0f;

        if ( MathF.Abs( rayDirection.Y ) < 0.0001f )
            return null;

        float t = ( planeY - rayOrigin.Y ) / rayDirection.Y;
        if ( t < 0 )
            return null;

        return rayOrigin + rayDirection * t;
    }

    // Ищем карту в заданной позиции, игнорируя совпавшие и переворачивающиеся карты
    private Entity? FindCardAtPosition( Vector3D<float> position )
    {
        foreach ( Entity entity in World.Filter<Card>().With<Transform>() )
        {
            ref Card card = ref World.Get<Card>( entity );
            if ( card.IsMatched || card.IsFlipping )
                continue;

            Transform transform = World.Get<Transform>( entity );
            if ( IsPointInCard( position, transform.Position ) )
            {
                return entity;
            }
        }

        return null;
    }

    // Проверяем, находится ли точка внутри границ карты (AABB проверка в 2D)
    private bool IsPointInCard( Vector3D<float> point, Vector3D<float> cardPosition )
    {
        const float cardSize = 0.9f;
        float halfSize = cardSize / 2f;

        float minX = cardPosition.X - halfSize;
        float maxX = cardPosition.X + halfSize;
        float minZ = cardPosition.Z - halfSize;
        float maxZ = cardPosition.Z + halfSize;

        return point.X >= minX && point.X <= maxX &&
               point.Z >= minZ && point.Z <= maxZ;
    }
}
