using Graphics.Structures;

namespace Graphics.Components;

/// <summary>
/// Texture2D component that references a TextureObject.
/// The actual texture logic is in TextureObject class.
/// </summary>
public struct Texture2D
{
    public TextureObject Texture { get; set; }
    
    /// <summary>
    /// Gets the OpenGL handle for the texture
    /// </summary>
    public readonly uint Handle => Texture?.Handle ?? 0;
    
    /// <summary>
    /// Gets the width of the texture
    /// </summary>
    public readonly int Width => Texture?.Width ?? 0;
    
    /// <summary>
    /// Gets the height of the texture
    /// </summary>
    public readonly int Height => Texture?.Height ?? 0;
}