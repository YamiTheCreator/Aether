using System.Numerics;
using Graphics.Components;
using Graphics.Input;
using Graphics.Windowing;
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
    public static Vector2 GetMousePosition()
    {
        Vector2 rawPos = Input.MousePosition;
        // On Retina displays, mouse coordinates are doubled
        return rawPos / 2f;
    }

    /// <summary>
    /// Converts mouse screen position to world coordinates using camera
    /// </summary>
    public static Vector2 ScreenToWorld( Vector2 mousePos, Camera camera )
    {
        float screenWidth = MainWindow.Width / 2f;
        float screenHeight = MainWindow.Height / 2f;

        float worldHalfHeight = camera.OrthographicSize;
        float worldHalfWidth = worldHalfHeight * ( screenWidth / screenHeight );

        float mouseX = ( mousePos.X / screenWidth ) * worldHalfWidth - worldHalfWidth;
        float mouseY = worldHalfHeight - ( mousePos.Y / screenHeight ) * worldHalfHeight;

        return new Vector2( mouseX, mouseY );
    }

    /// <summary>
    /// Converts world position to grid cell coordinates
    /// </summary>
    public static Vector2? WorldToCell( Vector2 worldPos, Vector2 gridOffset, float cellSize, int gridWidth,
        int gridHeight )
    {
        float localX = worldPos.X - gridOffset.X;
        float localY = worldPos.Y - gridOffset.Y;

        int cellX = ( int )( localX / cellSize );
        int cellY = ( int )( localY / cellSize );

        if ( cellX < 0 || cellX >= gridWidth || cellY < 0 || cellY >= gridHeight )
            return null;

        return new Vector2( cellX, cellY );
    }

    /// <summary>
    /// Checks if mouse button was just pressed (not held)
    /// </summary>
    public static bool IsMouseJustPressed( MouseButton button, ref bool previousState )
    {
        bool currentState = Input.IsMouseButtonDown( button );
        bool justPressed = currentState && !previousState;
        previousState = currentState;
        return justPressed;
    }
}