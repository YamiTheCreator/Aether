using Graphics.Structures;

namespace Graphics.Components;

public struct Font
{
    public uint Handle { get; set; }
    public float Scale { get; set; }
    public float LineHeight { get; set; }
    public Dictionary<char, (Glyph Glyph, uint TextureHandle)> GlyphCache { get; set; }
    public byte[] FontData { get; set; }
}