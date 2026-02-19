using Graphics;
using Silk.NET.Maths;
using Graphics.Components;
using MouseButton = Silk.NET.Input.MouseButton;

namespace GameUtils.Helpers;

/// <summary>
/// Helper for converting screen coordinates to world coordinates
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Gets normalized mouse position (accounts for Retina displays)
    /// </summary>
    public static Vector2D<float> GetMousePosition()
    {
        System.Numerics.Vector2 rawPos = WindowBase.Input.Mice[0].Position;
        
        // Get actual framebuffer size vs logical window size to detect scaling
        int frameBufferWidth = WindowBase.Width;
        int frameBufferHeight = WindowBase.Height;
        int logicalWidth = WindowBase.LogicalWidth;
        int logicalHeight = WindowBase.LogicalHeight;
        
        float scaleX = (float)frameBufferWidth / logicalWidth;
        float scaleY = (float)frameBufferHeight / logicalHeight;
        
        // Normalize coordinates based on actual scaling
        return new Vector2D<float>(rawPos.X / scaleX, rawPos.Y / scaleY);
    }

    /// <summary>
    /// Converts mouse screen position to world coordinates using camera
    /// </summary>
    public static Vector2D<float> ScreenToWorld( Vector2D<float> mousePos, Camera camera )
    {
        float screenWidth = WindowBase.LogicalWidth;
        float screenHeight = WindowBase.LogicalHeight;

        float worldHalfHeight = camera.OrthographicSize;
        float worldHalfWidth = worldHalfHeight * camera.AspectRatio;

        // Normalize mouse position to [-1, 1] range
        float normalizedX = (mousePos.X / screenWidth) * 2f - 1f;
        float normalizedY = 1f - (mousePos.Y / screenHeight) * 2f; // Flip Y axis

        // Convert to world coordinates
        float worldX = normalizedX * worldHalfWidth;
        float worldY = normalizedY * worldHalfHeight;

        return new Vector2D<float>( worldX, worldY );
    }

    /// <summary>
    /// Converts world position to grid cell coordinates
    /// </summary>
    public static Vector2D<float>? WorldToCell( Vector2D<float> worldPos, Vector2D<float> gridOffset, float cellSize, int gridWidth,
        int gridHeight )
    {
        float localX = worldPos.X - gridOffset.X;
        float localY = worldPos.Y - gridOffset.Y;

        int cellX = ( int )( localX / cellSize );
        int cellY = ( int )( localY / cellSize );

        if ( cellX < 0 || cellX >= gridWidth || cellY < 0 || cellY >= gridHeight )
            return null;

        return new Vector2D<float>( cellX, cellY );
    }

    /// <summary>
    /// Checks if mouse button was just pressed (not held)
    /// </summary>
    public static bool IsMouseJustPressed( MouseButton button, ref bool previousState )
    {
        bool currentState = WindowBase.Input.Mice[0].IsButtonPressed(button);
        bool justPressed = currentState && !previousState;
        previousState = currentState;
        return justPressed;
    }
}