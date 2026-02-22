using Aether.Core;
using Silk.NET.Maths;

namespace Graphics.Components;

public struct Sprite : Component
{
    public Material Material;

    public Vector2D<float> Size;

    public Vector2D<float> Pivot;

    public bool FlipX;

    public bool FlipY;

    public Vector4D<float> Color;

    public static Sprite Create( Material material, Vector2D<float> size )
    {
        return new Sprite
        {
            Material = material,
            Size = size,
            Pivot = new Vector2D<float>( 0.5f, 0.5f ),
            FlipX = false,
            FlipY = false,
            Color = new Vector4D<float>( 1, 1, 1, 1 )
        };
    }
}