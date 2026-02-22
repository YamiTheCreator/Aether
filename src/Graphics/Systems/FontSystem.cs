using Graphics.Structures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbTrueTypeSharp;
using FontComponent = Graphics.Components.Font;

namespace Graphics.Systems;

public class FontSystem
{
    private readonly GL _gl;

    public FontSystem(GL gl)
    {
        _gl = gl;
        _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
    }

    public unsafe FontComponent CreateFont(float fontSize = 48f, string fontPath = "src/Graphics/Assets/Fonts/Strogo.ttf")
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string solutionRoot = Path.GetFullPath(Path.Combine(basePath, "../../../../../"));
        string fullFontPath = Path.Combine(solutionRoot, fontPath);

        if (!File.Exists(fullFontPath))
            throw new FileNotFoundException($"Font not found: {fullFontPath}");

        byte[] fontData = File.ReadAllBytes(fullFontPath);
        StbTrueType.stbtt_fontinfo fontInfo = StbTrueType.CreateFont(fontData, 0);
        float scale = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, fontSize);

        int ascent = 0, descent = 0, lineGap = 0;
        StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);
        float lineHeight = (ascent - descent) * scale;

        return new FontComponent
        {
            Handle = 0,
            Scale = scale,
            LineHeight = lineHeight,
            GlyphCache = new Dictionary<char, (Glyph, uint)>(),
            FontData = fontData
        };
    }

    public unsafe (Glyph glyph, uint textureHandle) GetGlyph(ref FontComponent font, char c)
    {
        if (font.GlyphCache.TryGetValue(c, out (Glyph glyph, uint textureHandle) cached))
            return cached;

        StbTrueType.stbtt_fontinfo fontInfo = StbTrueType.CreateFont(font.FontData, 0);

        int advanceWidth = 0, leftSideBearing = 0;
        StbTrueType.stbtt_GetCodepointHMetrics(fontInfo, c, &advanceWidth, &leftSideBearing);

        int x0 = 0, y0 = 0, x1 = 0, y1 = 0;
        StbTrueType.stbtt_GetCodepointBitmapBox(fontInfo, c, font.Scale, font.Scale, &x0, &y0, &x1, &y1);

        int width = x1 - x0;
        int height = y1 - y0;

        Glyph glyph = new()
        {
            Size = new Vector2D<float>(width, height),
            Bearing = new Vector2D<float>(x0, -y1),
            Advance = advanceWidth * font.Scale,
            UvMin = Vector2D<float>.Zero,
            UvMax = Vector2D<float>.One
        };

        uint textureHandle = CreateGlyphTexture(fontInfo, c, width, height, font.Scale);

        font.GlyphCache[c] = (glyph, textureHandle);
        return (glyph, textureHandle);
    }

    public (float width, float height) MeasureText(ref FontComponent font, string text)
    {
        float width = 0;
        float height = font.LineHeight;

        foreach (char c in text)
        {
            if (c == '\n')
            {
                height += font.LineHeight;
                continue;
            }

            (Glyph glyph, _) = GetGlyph(ref font, c);
            width += glyph.Advance;
        }

        return (width, height);
    }

    public void DeleteFont(FontComponent font)
    {
        foreach ((Glyph _, uint textureHandle) in font.GlyphCache.Values)
        {
            _gl.DeleteTexture(textureHandle);
        }
    }

    private unsafe uint CreateGlyphTexture(StbTrueType.stbtt_fontinfo fontInfo, char c, int width, int height, float scale)
    {
        if (width <= 0 || height <= 0)
        {
            ReadOnlySpan<byte> emptyData = "\0\0\0\0"u8;
            
            // Use bind-based API (OpenGL 4.1 compatible)
            uint handle = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, handle);

            fixed (byte* ptr = emptyData)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 1, 1, 
                    0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }

            SetTextureParameters();
            _gl.BindTexture(TextureTarget.Texture2D, 0);
            return handle;
        }

        int bitmapSize = width * height;
        int rgbaSize = bitmapSize * 4;

        Span<byte> bitmap = bitmapSize <= 65536
            ? stackalloc byte[bitmapSize]
            : new byte[bitmapSize];

        Span<byte> rgbaData = rgbaSize <= 65536
            ? stackalloc byte[rgbaSize]
            : new byte[rgbaSize];

        fixed (byte* bitmapPtr = bitmap)
        {
            StbTrueType.stbtt_MakeCodepointBitmap(fontInfo, bitmapPtr, width, height, width, scale, scale, c);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcIndex = y * width + x;
                int dstIndex = (height - 1 - y) * width + x;
                byte alpha = bitmap[srcIndex];
                // Используем premultiplied alpha для правильного блендинга
                rgbaData[dstIndex * 4 + 0] = alpha;  // R = alpha (белый * alpha)
                rgbaData[dstIndex * 4 + 1] = alpha;  // G = alpha
                rgbaData[dstIndex * 4 + 2] = alpha;  // B = alpha
                rgbaData[dstIndex * 4 + 3] = alpha;  // A = alpha
            }
        }

        // Use bind-based API (OpenGL 4.1 compatible)
        uint textureHandle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, textureHandle);

        fixed (byte* ptr = rgbaData)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 
                0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }

        SetTextureParameters();
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        return textureHandle;
    }

    private void SetTextureParameters()
    {
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }

}