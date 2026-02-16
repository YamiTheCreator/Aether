using System.Numerics;
using Aether.Core;

namespace UI.Components;

/// <summary>
/// Universal button component with texture support.
/// </summary>
public struct Button(
    string text,
    Vector2 position,
    Vector2 size,
    Vector4? color = null,
    Vector4? textColor = null,
    bool useNineSlice = true,
    float borderSize = 8f )
    : IComponent
{
    public string Text = text;
    public Vector2 Position = position;
    public Vector2 Size = size;
    public Vector4 Color = color ?? new Vector4( 1, 1, 1, 1 );
    public Vector4 TextColor = textColor ?? new Vector4( 0, 0, 0, 1 ); // Default black text
    public ButtonState State = ButtonState.Normal;
    public bool UseNineSlice = useNineSlice;
    public float BorderSize = borderSize;

    public bool Contains( float x, float y )
    {
        return x >= Position.X && x <= Position.X + Size.X &&
               y >= Position.Y && y <= Position.Y + Size.Y;
    }
}

/// <summary>
/// Button visual state.
/// </summary>
public enum ButtonState
{
    Normal,
    Hovered,
    Pressed,
    Disabled
}