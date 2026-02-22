using Graphics.Structures;

namespace Graphics.Components;

public struct Texture2D
{
    public TextureObject Texture { get; set; }

    public readonly uint Handle => Texture?.Handle ?? 0;

    public readonly int Width => Texture?.Width ?? 0;

    public readonly int Height => Texture?.Height ?? 0;
}