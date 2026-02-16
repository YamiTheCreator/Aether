using System.Numerics;
using Aether.Core;
using Graphics.Helpers;

namespace Graphics.Components;

/// <summary>
/// Text component for rendering text in the world.
/// Requires Transform component for positioning.
/// </summary>
public struct TextComponent(
    string content,
    Vector4 color,
    float scale = 1f,
    TextAlignment alignment = TextAlignment.Left )
    : IComponent
{
    public readonly string Content = content;
    public Vector4 Color = color;
    public readonly float Scale = scale;
    public readonly TextAlignment Alignment = alignment;

    public static TextComponent Create( string content, Vector4 color, float scale = 1f )
        => new( content, color, scale, TextAlignment.Left );

    public static TextComponent CreateCentered( string content, Vector4 color, float scale = 1f )
        => new( content, color, scale, TextAlignment.Center );
}