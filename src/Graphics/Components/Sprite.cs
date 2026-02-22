using Aether.Core;
using Silk.NET.Maths;

namespace Graphics.Components;

/// <summary>
/// Sprite component for 2D rendering
/// Contains only data - vertices, indices, material reference
/// </summary>
public struct Sprite : Component
{
    /// <summary>
    /// Material reference (contains texture, color, shader)
    /// </summary>
    public Material Material;
    
    /// <summary>
    /// Size of the sprite
    /// </summary>
    public Vector2D<float> Size;
    
    /// <summary>
    /// Pivot point (0,0 = top-left, 0.5,0.5 = center, 1,1 = bottom-right)
    /// </summary>
    public Vector2D<float> Pivot;
    
    /// <summary>
    /// Flip horizontally
    /// </summary>
    public bool FlipX;
    
    /// <summary>
    /// Flip vertically
    /// </summary>
    public bool FlipY;
    
    /// <summary>
    /// Tint color (multiplied with texture)
    /// </summary>
    public Vector4D<float> Color;

    public static Sprite Create(Material material, Vector2D<float> size)
    {
        return new Sprite
        {
            Material = material,
            Size = size,
            Pivot = new Vector2D<float>(0.5f, 0.5f),
            FlipX = false,
            FlipY = false,
            Color = new Vector4D<float>(1, 1, 1, 1)
        };
    }
}
